var DownloadDataFilePlugin = {

  // Unityからbyte[]（ArrayBuffer）を受け取る関数
  // この関数名はDllImportで指定したものと一致させる
  DownloadDataFile: function(fileName, data, dataLength) {
    // Unityの共有メモリバッファから安全なデータのコピーを作成
    var bytes = new Uint8Array(dataLength);
    bytes.set(new Uint8Array(Module.HEAPU8.buffer, data, dataLength));
    
    // Uint8Arrayから直接Blobを作成
    var blob = new Blob([bytes], { type: "application/octet-stream" });
    
    // BlobのURLを作成し、ダウンロードリンクを生成
    var url = URL.createObjectURL(blob);
    var a = document.createElement("a");
    document.body.appendChild(a);
    a.style = "display: none";
    a.href = url;
    a.download = UTF8ToString(fileName);
    a.click();
    
    // 後処理
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  }

};


mergeInto(LibraryManager.library, DownloadDataFilePlugin);