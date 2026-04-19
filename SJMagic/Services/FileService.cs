using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinForms = System.Windows.Forms;

namespace SJMagic.Services
{
    public class FileService
    {
        private readonly string[] _imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
        private readonly string[] _videoExtensions = { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".webm" };

        public string SelectFolder()
        {
            using (var dialog = new WinForms.FolderBrowserDialog())
            {
                dialog.Description = "미디어 파일이 있는 폴더를 선택하세요";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }
            }
            return null;
        }

        public string SelectFile(string filter = "Media Files|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.webm;*.jpg;*.jpeg;*.png;*.bmp;*.gif")
        {
            using (var dialog = new WinForms.OpenFileDialog())
            {
                dialog.Filter = filter;
                dialog.Title = "미디어 파일을 선택하세요";
                if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    return dialog.FileName;
                }
            }
            return null;
        }

        public List<FileInfo> GetMediaFiles(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return new List<FileInfo>();

            var dirInfo = new DirectoryInfo(folderPath);
            var allExtensions = _imageExtensions.Concat(_videoExtensions).ToArray();

            return dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                          .Where(f => allExtensions.Contains(f.Extension.ToLower()))
                          .OrderBy(f => f.Name)
                          .ToList();
        }

        public bool IsVideo(FileInfo file)
        {
            return _videoExtensions.Contains(file.Extension.ToLower());
        }

        public bool IsImage(FileInfo file)
        {
            return _imageExtensions.Contains(file.Extension.ToLower());
        }
    }
}
