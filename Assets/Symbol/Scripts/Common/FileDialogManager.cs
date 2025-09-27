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
                // FileImporterCaptureClick�ŊJ�����E�B���h�E���L�����Z���ŕ��Ă�
                // Unity���ɒʒm������@�������悤�Ȃ̂ŁACts�������Ă��鎞�͈�U�L�����Z�����s�B
                // Unity�̃{�^���̕\�����uselect file�v����ucancel select file�v�Ȃǂɕς��Ă����̂��ǂ������B
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
                // �L�����Z�������܂őҋ@
                await UniTask.WaitUntilCanceled( Cts.Token );
            }
            catch(System.OperationCanceledException)
            {
                // �L�����Z�����ꂽ�ꍇ�͗�O�𖳎�
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
                    //Debug.Log( "Byte�z����t�@�C���ɕۑ����܂���: " + dlg.FileName );
                }
                catch(System.Exception e)
                {
                    //Debug.LogError( "�t�@�C���̕ۑ��Ɏ��s���܂���: " + e.Message );
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
                // FileImporterCaptureClick�ŊJ�����E�B���h�E���L�����Z���ŕ��Ă�
                // Unity���ɒʒm������@�������悤�Ȃ̂ŁACts�������Ă��鎞�͈�U�L�����Z�����s�B
                // Unity�̃{�^���̕\�����uselect file�v����ucancel select file�v�Ȃǂɕς��Ă����̂��ǂ������B
                Cts.Cancel();
                return "";
            }


            FileName = "";

#if UNITY_WEBGL && !UNITY_EDITOR
            Cts = new System.Threading.CancellationTokenSource();

            FileImporterCaptureClick();

            try
            {
                // �L�����Z�������܂őҋ@
                await UniTask.WaitUntilCanceled( Cts.Token );
            }
            catch(System.OperationCanceledException)
            {
                // �L�����Z�����ꂽ�ꍇ�͗�O�𖳎�
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
                    //Debug.Log( "Byte�z����t�@�C���ɕۑ����܂���: " + dlg.FileName );
                }
                catch(System.Exception e)
                {
                    //Debug.LogError( "�t�@�C���̕ۑ��Ɏ��s���܂���: " + e.Message );
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

        // jslib����SendMessage�ŌĂяo���B�ÓI���\�b�h���ƌĂяo���Ȃ��̂Œ���
        public void FileSelected( string url )
        {
            FileName = url;
            Cts?.Cancel(); // �L�����Z���g�[�N���𔭉΂����đҋ@������
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

