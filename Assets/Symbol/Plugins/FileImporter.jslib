var FileImporterPlugin = {
  FileImporterCaptureClick: function() {
    if (!document.getElementById('FileImporter')) {
      var fileInput = document.createElement('input');
      fileInput.setAttribute('type', 'file');
      fileInput.setAttribute('id', 'FileImporter');
      fileInput.style.visibility = 'hidden';
      fileInput.onclick = function (event) {
        this.value = null;
      };
      fileInput.onchange = function (event) {
        // ファイル名とURLの両方をUnityに送信する
        // ファイル名を取得
        var fileName = event.target.files[0].name;
        // URLを作成
        var fileUrl = URL.createObjectURL(event.target.files[0]);
        // ファイル名とURLをカンマ区切りで送信 FileDialogManager
        SendMessage('SymbolSystemManager', 'FileSelected', fileName + ',' + fileUrl);
      }
      document.body.appendChild(fileInput);
    }

    // 修正箇所：イベントリスナーを削除するのではなく、即座にクリックを実行する
    document.getElementById('FileImporter').click();
  }
};
mergeInto(LibraryManager.library, FileImporterPlugin);