using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using Ionic.Zip;

using Octokit;

namespace OBSLauncher {
  public partial class Window : Form {
    string url = string.Empty;
    public Window() {
      InitializeComponent();
    }

    private string GetDownloadUrl() {
      string pattern = @"^OBS-Studio-\d+(\.\d+)*\.zip$";
      GitHubClient ghClient = new GitHubClient(new ProductHeaderValue("OBS-Launcher"));

      var release = ghClient.Repository.Release.GetLatest("obsproject", "obs-studio").Result;
      foreach (var asset in release.Assets) {
        if (Regex.IsMatch(asset.Name, pattern)) {
          return asset.BrowserDownloadUrl;
        }
      }
      return string.Empty;
    }

    private async Task DownloadRelease(string url) {
      using (var client = new HttpClientDownloadWithProgress(url, Path.GetFileName(url))) {
        client.ProgressChanged += (totalFileSize, totalBytesDownloaded, percentage) => {
          progressBar.Value = (int)percentage / 2;
        };

        await client.StartDownload();
      }
    }

    private async Task ExtractZip(string filename) {
      if (string.IsNullOrEmpty(filename)) {
        throw new Exception($"{filename} does not exists!");
      }

      using (var zipfile = ZipFile.Read(filename)) {
        zipfile.ExtractProgress += ZipExtractProgress;
        await Task.Run(() => zipfile.ExtractAll(AppDomain.CurrentDomain.BaseDirectory, ExtractExistingFileAction.OverwriteSilently));
      }
    }

    private void ZipExtractProgress(object sender, ExtractProgressEventArgs e) {
      if (e.TotalBytesToTransfer > 0) {
        int progress = (int)(e.BytesTransferred / e.TotalBytesToTransfer * 100 / 2) + 50;
        //Invoke((MethodInvoker)(() => progressBar.Value = progress));
        progressBar.Value = progress;
      }
    }

    private async void Window_Load(object sender, EventArgs e) {
      var result = MessageBox.Show("OBS Studio is not found\nDo you want to download it?", "Download OBS Studio?", MessageBoxButtons.YesNo);

      if (result == DialogResult.Yes) {
        url = GetDownloadUrl();
        var filename = Path.GetFileName(url);
        if (!File.Exists(filename)) {
          // Download OBS Studio
          label1.Text = "Downloading latest OBS Studio...";
          await DownloadRelease(url);
          // Wait 0.3 second
          await Task.Delay(300);
        }
        // Extract ZIP
        label1.Text = "Extracting OBS Studio...";
        await ExtractZip(Path.GetFileName(url));
        // Done
        Close();
      } else {
        // Close OBS Launcher
        Close();
        Environment.Exit(0);
      }
    }
  }
}
