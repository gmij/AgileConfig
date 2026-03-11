import { request } from 'umi';

export async function createBackup() {
  return request('backup/CreateBackup', {
    method: 'POST',
  });
}

export async function listBackups() {
  return request('backup/List', {
    method: 'GET',
  });
}

export async function downloadBackup(fileName: string) {
  return request(`backup/Download?fileName=${encodeURIComponent(fileName)}`, {
    method: 'GET',
    responseType: 'blob',
  });
}

export async function restoreFromFile(file: File) {
  const formData = new FormData();
  formData.append('file', file);

  return request('backup/Restore', {
    method: 'POST',
    data: formData,
    requestType: 'form',
  });
}

export async function restoreFromHistory(fileName: string) {
  return request('backup/RestoreFromHistory', {
    method: 'POST',
    params: { fileName },
  });
}

export async function deleteBackup(fileName: string) {
  return request('backup/Delete', {
    method: 'POST',
    params: { fileName },
  });
}
