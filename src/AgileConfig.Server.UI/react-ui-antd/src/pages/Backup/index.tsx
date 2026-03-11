import React, { useState, useEffect } from 'react';
import { PageContainer } from '@ant-design/pro-layout';
import { Card, Button, Table, Upload, message, Modal, Space, Typography, Divider } from 'antd';
import { SaveOutlined, DownloadOutlined, UploadOutlined, ReloadOutlined, DeleteOutlined, RollbackOutlined } from '@ant-design/icons';
import { useIntl } from 'umi';
import {
  createBackup,
  listBackups,
  downloadBackup,
  restoreFromFile,
  restoreFromHistory,
  deleteBackup,
} from './service';
import type { UploadFile } from 'antd/es/upload/interface';

const { Text, Title } = Typography;
const { Dragger } = Upload;

interface BackupInfo {
  fileName: string;
  filePath: string;
  fileSize: number;
  createTime: string;
  appCount: number;
}

interface RestoreStatistics {
  appsImported: number;
  configsImported: number;
  appsUpdated: number;
  configsUpdated: number;
  errors: number;
  errorMessages: string[];
}

const Backup: React.FC = () => {
  const intl = useIntl();
  const [backupLoading, setBackupLoading] = useState(false);
  const [backups, setBackups] = useState<BackupInfo[]>([]);
  const [loading, setLoading] = useState(false);
  const [latestBackup, setLatestBackup] = useState<BackupInfo | null>(null);

  const loadBackups = async () => {
    setLoading(true);
    try {
      const result = await listBackups();
      if (result.success) {
        setBackups(result.data || []);
        if (result.data && result.data.length > 0) {
          setLatestBackup(result.data[0]);
        }
      } else {
        message.error(result.message || intl.formatMessage({ id: 'failed' }));
      }
    } catch (error) {
      message.error(intl.formatMessage({ id: 'failed' }));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadBackups();
  }, []);

  const handleCreateBackup = async () => {
    setBackupLoading(true);
    try {
      message.loading({
        content: intl.formatMessage({ id: 'pages.backup.backup.creating' }),
        key: 'createBackup',
      });
      const result = await createBackup();
      if (result.success) {
        message.success({
          content: intl.formatMessage({ id: 'pages.backup.backup.create_success' }),
          key: 'createBackup',
        });
        loadBackups();
      } else {
        message.error({
          content: result.message || intl.formatMessage({ id: 'pages.backup.backup.create_fail' }),
          key: 'createBackup',
        });
      }
    } catch (error) {
      message.error({
        content: intl.formatMessage({ id: 'pages.backup.backup.create_fail' }),
        key: 'createBackup',
      });
    } finally {
      setBackupLoading(false);
    }
  };

  const handleDownload = async (fileName: string) => {
    try {
      const blob = await downloadBackup(fileName);
      const url = window.URL.createObjectURL(new Blob([blob]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileName);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      message.error(intl.formatMessage({ id: 'failed' }));
    }
  };

  const handleRestoreFromHistory = (fileName: string) => {
    Modal.confirm({
      title: intl.formatMessage({ id: 'pages.backup.restore.restore' }),
      content: intl.formatMessage({ id: 'pages.backup.restore.confirmRestore' }),
      onOk: async () => {
        try {
          message.loading({
            content: intl.formatMessage({ id: 'pages.backup.restore.restoring' }),
            key: 'restore',
          });
          const result = await restoreFromHistory(fileName);
          if (result.success) {
            showRestoreResult(result.data);
            message.success({
              content: intl.formatMessage({ id: 'pages.backup.restore.restore_success' }),
              key: 'restore',
            });
          } else {
            message.error({
              content: result.message || intl.formatMessage({ id: 'pages.backup.restore.restore_fail' }),
              key: 'restore',
            });
          }
        } catch (error) {
          message.error({
            content: intl.formatMessage({ id: 'pages.backup.restore.restore_fail' }),
            key: 'restore',
          });
        }
      },
    });
  };

  const handleDelete = (fileName: string) => {
    Modal.confirm({
      title: intl.formatMessage({ id: 'pages.backup.restore.delete' }),
      content: intl.formatMessage({ id: 'pages.backup.restore.confirmDelete' }),
      onOk: async () => {
        try {
          const result = await deleteBackup(fileName);
          if (result.success) {
            message.success(intl.formatMessage({ id: 'pages.backup.restore.delete_success' }));
            loadBackups();
          } else {
            message.error(result.message || intl.formatMessage({ id: 'pages.backup.restore.delete_fail' }));
          }
        } catch (error) {
          message.error(intl.formatMessage({ id: 'pages.backup.restore.delete_fail' }));
        }
      },
    });
  };

  const showRestoreResult = (stats: RestoreStatistics) => {
    Modal.info({
      title: intl.formatMessage({ id: 'pages.backup.restore.restore_success' }),
      width: 600,
      content: (
        <div>
          <p>
            <Text strong>{intl.formatMessage({ id: 'pages.backup.stats.appsImported' })}:</Text> {stats.appsImported}
          </p>
          <p>
            <Text strong>{intl.formatMessage({ id: 'pages.backup.stats.appsUpdated' })}:</Text> {stats.appsUpdated}
          </p>
          <p>
            <Text strong>{intl.formatMessage({ id: 'pages.backup.stats.configsImported' })}:</Text> {stats.configsImported}
          </p>
          <p>
            <Text strong>{intl.formatMessage({ id: 'pages.backup.stats.configsUpdated' })}:</Text> {stats.configsUpdated}
          </p>
          {stats.errors > 0 && (
            <>
              <p>
                <Text strong type="danger">{intl.formatMessage({ id: 'pages.backup.stats.errors' })}:</Text> {stats.errors}
              </p>
              {stats.errorMessages && stats.errorMessages.length > 0 && (
                <div style={{ maxHeight: '200px', overflow: 'auto' }}>
                  {stats.errorMessages.map((msg, idx) => (
                    <p key={idx} style={{ color: 'red', fontSize: '12px' }}>
                      {msg}
                    </p>
                  ))}
                </div>
              )}
            </>
          )}
        </div>
      ),
    });
  };

  const uploadProps = {
    name: 'file',
    multiple: false,
    accept: '.zip',
    showUploadList: false,
    beforeUpload: (file: UploadFile) => {
      if (!file.name.endsWith('.zip')) {
        message.error(intl.formatMessage({ id: 'pages.backup.restore.uploadDesc' }));
        return false;
      }

      Modal.confirm({
        title: intl.formatMessage({ id: 'pages.backup.restore.restore' }),
        content: intl.formatMessage({ id: 'pages.backup.restore.confirmRestore' }),
        onOk: async () => {
          try {
            message.loading({
              content: intl.formatMessage({ id: 'pages.backup.restore.restoring' }),
              key: 'restore',
            });
            const result = await restoreFromFile(file as any);
            if (result.success) {
              showRestoreResult(result.data);
              message.success({
                content: intl.formatMessage({ id: 'pages.backup.restore.restore_success' }),
                key: 'restore',
              });
            } else {
              message.error({
                content: result.message || intl.formatMessage({ id: 'pages.backup.restore.restore_fail' }),
                key: 'restore',
              });
            }
          } catch (error) {
            message.error({
              content: intl.formatMessage({ id: 'pages.backup.restore.restore_fail' }),
              key: 'restore',
            });
          }
        },
      });

      return false;
    },
  };

  const formatFileSize = (bytes: number) => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(2) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
  };

  const columns = [
    {
      title: intl.formatMessage({ id: 'pages.backup.restore.fileName' }),
      dataIndex: 'fileName',
      key: 'fileName',
    },
    {
      title: intl.formatMessage({ id: 'pages.backup.restore.fileSize' }),
      dataIndex: 'fileSize',
      key: 'fileSize',
      render: (size: number) => formatFileSize(size),
    },
    {
      title: intl.formatMessage({ id: 'pages.backup.restore.createTime' }),
      dataIndex: 'createTime',
      key: 'createTime',
      render: (time: string) => new Date(time).toLocaleString(),
    },
    {
      title: intl.formatMessage({ id: 'pages.backup.restore.appCount' }),
      dataIndex: 'appCount',
      key: 'appCount',
    },
    {
      title: intl.formatMessage({ id: 'pages.backup.restore.operations' }),
      key: 'operations',
      render: (_: any, record: BackupInfo) => (
        <Space>
          <Button
            type="link"
            icon={<RollbackOutlined />}
            onClick={() => handleRestoreFromHistory(record.fileName)}
          >
            {intl.formatMessage({ id: 'pages.backup.restore.restore' })}
          </Button>
          <Button
            type="link"
            icon={<DownloadOutlined />}
            onClick={() => handleDownload(record.fileName)}
          >
            {intl.formatMessage({ id: 'pages.backup.backup.download' })}
          </Button>
          <Button
            type="link"
            danger
            icon={<DeleteOutlined />}
            onClick={() => handleDelete(record.fileName)}
          >
            {intl.formatMessage({ id: 'pages.backup.restore.delete' })}
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <PageContainer title={intl.formatMessage({ id: 'pages.backup.title' })}>
      <div style={{ display: 'flex', gap: '16px' }}>
        {/* Backup Card */}
        <Card
          title={intl.formatMessage({ id: 'pages.backup.backup.title' })}
          style={{ flex: 1 }}
          extra={
            <Button
              type="primary"
              icon={<SaveOutlined />}
              loading={backupLoading}
              onClick={handleCreateBackup}
            >
              {intl.formatMessage({ id: 'pages.backup.backup.create' })}
            </Button>
          }
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text type="secondary">{intl.formatMessage({ id: 'pages.backup.backup.directory' })}:</Text>
              <br />
              <Text code>/backup/yyyy-MM-dd/</Text>
            </div>
            <Divider />
            {latestBackup && (
              <div>
                <Text strong>{intl.formatMessage({ id: 'pages.backup.backup.latest' })}:</Text>
                <br />
                <Text>{latestBackup.fileName}</Text>
                <br />
                <Text type="secondary">
                  {latestBackup.appCount} {intl.formatMessage({ id: 'pages.backup.backup.appCount' })}
                </Text>
                <br />
                <Button
                  type="link"
                  icon={<DownloadOutlined />}
                  onClick={() => handleDownload(latestBackup.fileName)}
                  style={{ paddingLeft: 0 }}
                >
                  {intl.formatMessage({ id: 'pages.backup.backup.download' })}
                </Button>
              </div>
            )}
          </Space>
        </Card>

        {/* Restore Card */}
        <Card
          title={intl.formatMessage({ id: 'pages.backup.restore.title' })}
          style={{ flex: 1 }}
          extra={
            <Button icon={<ReloadOutlined />} onClick={loadBackups}>
              {intl.formatMessage({ id: 'refreshing' })}
            </Button>
          }
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            <Dragger {...uploadProps}>
              <p className="ant-upload-drag-icon">
                <UploadOutlined style={{ fontSize: '48px', color: '#1890ff' }} />
              </p>
              <p className="ant-upload-text">
                {intl.formatMessage({ id: 'pages.backup.restore.uploadTip' })}
              </p>
              <p className="ant-upload-hint">
                {intl.formatMessage({ id: 'pages.backup.restore.uploadDesc' })}
              </p>
            </Dragger>
          </Space>
        </Card>
      </div>

      {/* History Table */}
      <Card
        title={intl.formatMessage({ id: 'pages.backup.restore.history' })}
        style={{ marginTop: '16px' }}
      >
        <Table
          columns={columns}
          dataSource={backups}
          rowKey="fileName"
          loading={loading}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} items`,
          }}
        />
      </Card>
    </PageContainer>
  );
};

export default Backup;
