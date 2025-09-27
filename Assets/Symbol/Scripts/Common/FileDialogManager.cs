using Cysharp.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace SB
{
    public class FileDialogManager : MonoBehaviour
    {
        [DllImport( "__Internal" )]
        private static extern void FileImporterCaptureClick();

        [DllImport( "__Internal" )]
        private static extern void DownloadTextFile( string fileName, string textContent );


        private static System.Threading.CancellationTokenSource Cts = null;
        private static bool IsOpen = false;

        public static string FileName = "";

        public static FileDialogManager Instance = null;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad( gameObject );
            }
            else
            {
                Destroy( gameObject );
            }
        }

        private void Update()
        {
        }

        public async UniTask<string> OpenAsync()
        {
            //if(Cts != null) return "";
            if(Cts != null)
            {
                // FileImporterCaptureClickで開いたウィンドウをキャンセルで閉じても
                // Unity側に通知する方法が無いようなので、Ctsが生きている時は一旦キャンセル発行。
                // Unityのボタンの表示を「select file」から「cancel select file」などに変えておくのが良さそう。
                Cts.Cancel();
                return "";
            }
            
            FileName = "";

#if UNITY_WEBGL && !UNITY_EDITOR
            Cts = new System.Threading.CancellationTokenSource();
            IsOpen = true;

            FileImporterCaptureClick();

            try
            {
                //await UniTask.WaitUntil( () => !IsOpen );
                // キャンセルされるまで待機
                await UniTask.WaitUntilCanceled( Cts.Token );
            }
            catch(System.OperationCanceledException)
            {
                // キャンセルされた場合は例外を無視
            }
            finally
            {
                Cts.Dispose();
                Cts = null;
                IsOpen = false;
            }

            return FileName;
#else
            var dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "All files(*.*)|*.*";
            dlg.CheckFileExists = false;
            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // 
                System.Windows.Forms.MessageBox.Show( dlg.FileName );

                try
                {
                    FileSelected( dlg.FileName );
                    //File.WriteAllBytes( dlg.FileName, decryptedData );
                    //Debug.Log( "Byte配列をファイルに保存しました: " + dlg.FileName );
                }
                catch(System.Exception e)
                {
                    //Debug.LogError( "ファイルの保存に失敗しました: " + e.Message );
                }
            }
            else
            {
                //
            }
            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}{dlg.FileName}" );
            return FileName;
#endif
        }

        public async UniTask<string> OpenFolderAsync()
        {
            //if(Cts != null) return "";
            if(Cts != null)
            {
                // FileImporterCaptureClickで開いたウィンドウをキャンセルで閉じても
                // Unity側に通知する方法が無いようなので、Ctsが生きている時は一旦キャンセル発行。
                // Unityのボタンの表示を「select file」から「cancel select file」などに変えておくのが良さそう。
                Cts.Cancel();
                return "";
            }


            FileName = "";

#if UNITY_WEBGL && !UNITY_EDITOR
            Cts = new System.Threading.CancellationTokenSource();

            FileImporterCaptureClick();

            try
            {
                // キャンセルされるまで待機
                await UniTask.WaitUntilCanceled( Cts.Token );
            }
            catch(System.OperationCanceledException)
            {
                // キャンセルされた場合は例外を無視
            }
            finally
            {
                Cts.Dispose();
                Cts = null;
            }

            return FileName;
#else

            var dlg = new FolderBrowserDialog();
            dlg.SelectedPath = "c:\\";
            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // 
                System.Windows.Forms.MessageBox.Show( dlg.SelectedPath );

                try
                {
                    FileSelected( dlg.SelectedPath );
                    //File.WriteAllBytes( dlg.FileName, decryptedData );
                    //Debug.Log( "Byte配列をファイルに保存しました: " + dlg.FileName );
                }
                catch(System.Exception e)
                {
                    //Debug.LogError( "ファイルの保存に失敗しました: " + e.Message );
                }
            }
            else
            {
                //
            }
            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}{dlg.SelectedPath}" );
            return FileName;
#endif
        }

        // jslibからSendMessageで呼び出し。静的メソッドだと呼び出せないので注意
        public void FileSelected( string url )
        {
            FileName = url;
            Cts?.Cancel(); // キャンセルトークンを発火させて待機を解除
            IsOpen = false;
            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}FileSelected:{url}" );
        }

        public async UniTask<int> DownloadTextFileAsync( string filename, string textdata )
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            DownloadTextFile( $"{filename}", textdata );
#else
            var url = await FileDialogManager.Instance.OpenFolderAsync();
            if(url == null || url == "") return 1;

            File.WriteAllText( $"{url}/{filename}", textdata );
#endif
            return 0;
        }
    }
}

