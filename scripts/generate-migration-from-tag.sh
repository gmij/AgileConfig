#!/bin/bash
# Script to generate EF Core migrations based on git tags
# Usage: ./generate-migration-from-tag.sh <tag_name>
#
# This script:
# 1. Checks out the specified git tag
# 2. Generates a new EF Core migration named after the tag version
# 3. Returns to the previous branch
#
# Example: ./generate-migration-from-tag.sh v1.11.0

set -e

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if tag name is provided
if [ -z "$1" ]; then
    echo -e "${RED}Error: Tag name is required${NC}"
    echo "Usage: $0 <tag_name>"
    echo "Example: $0 v1.11.0"
    exit 1
fi

TAG_NAME=$1
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
EFCORE_PROJECT="$REPO_ROOT/src/AgileConfig.Server.Data.EFCore"

# Verify tag exists
if ! git rev-parse "$TAG_NAME" >/dev/null 2>&1; then
    echo -e "${RED}Error: Tag '$TAG_NAME' does not exist${NC}"
    echo "Available tags:"
    git tag -l | sort -V
    exit 1
fi

# Extract version number from tag (remove 'v' prefix if present)
VERSION=$(echo "$TAG_NAME" | sed 's/^v//' | tr '.' '_')
MIGRATION_NAME="V${VERSION}"

echo -e "${GREEN}=== EF Core Migration Generator ===${NC}"
echo "Tag: $TAG_NAME"
echo "Migration name: $MIGRATION_NAME"
echo "Project: $EFCORE_PROJECT"
echo ""

# Save current branch
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
echo -e "${YELLOW}Current branch: $CURRENT_BRANCH${NC}"

# Checkout the tag
echo -e "${YELLOW}Checking out tag: $TAG_NAME${NC}"
git checkout "$TAG_NAME"

# Change to EF Core project directory
cd "$EFCORE_PROJECT"

# Generate migration
echo -e "${YELLOW}Generating migration: $MIGRATION_NAME${NC}"
dotnet ef migrations add "$MIGRATION_NAME" --output-dir Migrations

# Check if migration was created
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Migration generated successfully${NC}"
    echo ""
    echo "Migration files created:"
    ls -lh Migrations/*${MIGRATION_NAME}*
else
    echo -e "${RED}✗ Failed to generate migration${NC}"
    git checkout "$CURRENT_BRANCH"
    exit 1
fi

# Return to original branch
echo ""
echo -e "${YELLOW}Returning to branch: $CURRENT_BRANCH${NC}"
git checkout "$CURRENT_BRANCH"

echo ""
echo -e "${GREEN}=== Migration generation complete ===${NC}"
echo ""
echo "Next steps:"
echo "1. Review the generated migration files"
echo "2. Test the migration: dotnet ef database update"
echo "3. Commit the migration files to the repository"
echo ""
