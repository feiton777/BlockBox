var DownloadTextFilePlugin = {

  DownloadTextFile: function(fileName, textContent) {
    var text = UTF8ToString(textContent);
    var blob = new Blob([text], { type: 'text/plain' });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = UTF8ToString(fileName);
    a.click();
    URL.revokeObjectURL(url);
  }

};

mergeInto(LibraryManager.library, DownloadTextFilePlugin);