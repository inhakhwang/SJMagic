using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SJMagic.Base;
using SJMagic.Services;

namespace SJMagic.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly FileService _fileService;
        private readonly MediaService _mediaService;
        private readonly ImageService _imageService;

        private ObservableCollection<FileInfo> _mediaFiles;
        public ObservableCollection<FileInfo> MediaFiles
        {
            get => _mediaFiles;
            set => SetProperty(ref _mediaFiles, value);
        }

        private FileInfo _selectedMedia;
        public FileInfo SelectedMedia
        {
            get => _selectedMedia;
            set
            {
                if (SetProperty(ref _selectedMedia, value))
                {
                    OnMediaSelected();
                }
            }
        }

        private string _toolPanelTitle = "🛠️ Tools";
        public string ToolPanelTitle
        {
            get => _toolPanelTitle;
            set => SetProperty(ref _toolPanelTitle, value);
        }

        private double _imageRotationAngle = 0;
        public double ImageRotationAngle
        {
            get => _imageRotationAngle;
            set => SetProperty(ref _imageRotationAngle, value);
        }

        private double _imageScaleX = 1;
        public double ImageScaleX
        {
            get => _imageScaleX;
            set => SetProperty(ref _imageScaleX, value);
        }

        private double _imageScaleY = 1;
        public double ImageScaleY
        {
            get => _imageScaleY;
            set => SetProperty(ref _imageScaleY, value);
        }

        private bool _isVideoVisible;
        public bool IsVideoVisible
        {
            get => _isVideoVisible;
            set => SetProperty(ref _isVideoVisible, value);
        }

        private bool _isImageVisible;
        public bool IsImageVisible
        {
            get => _isImageVisible;
            set => SetProperty(ref _isImageVisible, value);
        }

        private string _logContent;
        public string LogContent
        {
            get => _logContent;
            set => SetProperty(ref _logContent, value);
        }

        public ICommand OpenFolderCommand { get; }
        public ICommand ConvertCommand { get; }
        public ICommand RotateImageCommand { get; }
        public ICommand FlipImageCommand { get; }
        public ICommand SaveImageCommand { get; }

        public MainViewModel()
        {
            _fileService = new FileService();
            _mediaService = new MediaService();
            _imageService = new ImageService();
            MediaFiles = new ObservableCollection<FileInfo>();

            OpenFolderCommand = new RelayCommand(OpenFolder);
            ConvertCommand = new RelayCommand(async () => await ConvertSelectedMedia(), () => SelectedMedia != null && _fileService.IsVideo(SelectedMedia));
            
            RotateImageCommand = new RelayCommand<string>(dir => {
                ImageRotationAngle += (dir == "CW" ? 90 : -90);
            });

            FlipImageCommand = new RelayCommand<string>(axis => {
                if (axis == "H") ImageScaleX *= -1;
                else ImageScaleY *= -1;
            });

            SaveImageCommand = new RelayCommand(async () => await SaveProcessedImage());

            Log("SJMagic Studio 준비 완료.", "INFO");
        }

        private void OpenFolder()
        {
            var path = _fileService.SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
                var files = _fileService.GetMediaFiles(path);
                MediaFiles.Clear();
                foreach (var file in files) MediaFiles.Add(file);
                Log($"{files.Count}개의 파일을 불러왔습니다.", "SUCCESS");
            }
        }

        private void OnMediaSelected()
        {
            // Reset transforms
            ImageRotationAngle = 0;
            ImageScaleX = 1;
            ImageScaleY = 1;

            if (SelectedMedia == null)
            {
                IsVideoVisible = false;
                IsImageVisible = false;
                ToolPanelTitle = "🛠️ Tools";
                return;
            }

            IsVideoVisible = _fileService.IsVideo(SelectedMedia);
            IsImageVisible = _fileService.IsImage(SelectedMedia);
            
            ToolPanelTitle = IsVideoVisible ? "🎬 Video Tools" : "🖼️ Image Tools";
            
            Log($"선택됨: {SelectedMedia.Name} ({(IsVideoVisible ? "Video" : "Image")})", "EVENT");
        }

        private async Task ConvertSelectedMedia()
        {
            if (SelectedMedia == null || !IsVideoVisible) return;

            string inputPath = SelectedMedia.FullName;
            string outputDir = Path.Combine(SelectedMedia.DirectoryName, "Converted");
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(SelectedMedia.Name);
            string outputPath = Path.Combine(outputDir, $"{fileNameWithoutExt}_EditorFriendly.mp4");

            bool success = await _mediaService.ConvertToEditorFriendly(inputPath, outputPath, msg => Log(msg, "PROCESS"));
            
            if (success)
            {
                Log($"영상 변환 완료: {outputPath}", "SUCCESS");
            }
        }

        private async Task SaveProcessedImage()
        {
            if (SelectedMedia == null || !IsImageVisible) return;

            string inputPath = SelectedMedia.FullName;
            string outputDir = Path.Combine(SelectedMedia.DirectoryName, "Converted");
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            string ext = Path.GetExtension(SelectedMedia.Name);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(SelectedMedia.Name);
            string outputPath = Path.Combine(outputDir, $"{fileNameWithoutExt}_Processed{ext}");

            Log($"이미지 저장 중: {outputPath}", "PROCESS");
            
            await Task.Run(() => {
                bool success = _imageService.SaveTransformedImage(inputPath, outputPath, ImageRotationAngle, ImageScaleX, ImageScaleY);
                if (success) Log($"이미지 저장 완료: {outputPath}", "SUCCESS");
                else Log("이미지 저장 실패!", "ERROR");
            });
        }

        public void Log(string message, string level = "INFO")
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            LogContent += $"[{timestamp}] [{level}] {message}{Environment.NewLine}";
        }
    }
}
