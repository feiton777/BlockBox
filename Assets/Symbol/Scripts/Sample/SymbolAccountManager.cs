using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
using System.Net;
using System.Net.Http;
using System.Text;
using System;
using System.IO;
using System.Runtime.InteropServices;
using AOT;

using System.Linq;

using SymbolSdk;
using SymbolSdk.Symbol;
using Cysharp.Threading.Tasks;
using SymbolEntity.Account;
using NUnit.Framework.Interfaces;

//using UniRx;

namespace SB
{
    public class SymbolAccountManager : MonoBehaviour
    {
        [System.Serializable]
        public class AccountJson
        {
            public string Address;
            public string PublicKey;
            public string PrivateKey;
        }

        public static SymbolAccountManager Instance = null;

        public const string FileName = "key";

        public PublicKey AlicePublicKey { private set; get; } = null;
        public UnresolvedAddress AliceAddress { private set; get; } = null;

        public AccountDatum AliceAccountDatum { private set; get; } = null;

        public double AliceXYM = 0;

        private float m_UpdateTimer = 5.0f;

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

        void Start()
        {
            LoadAccount();
            //if(AliceAddress == null)
            //{
            //    GenerateAccount();
            //}
        }

        void Update()
        {
            // 一定周期でアカウントの状態取得
            if(AliceAddress != null)
            {
                m_UpdateTimer -= Time.deltaTime;
                if(m_UpdateTimer < 0.0f)
                {
                    UpdateAccountDataAsync().Forget();
                    m_UpdateTimer = 5.0f;
                }
            }
        }

        public void LoadAccount()
        {
            var key = LoadPrivateKey();
            if(key == "")
            {
                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not load Key" );
                return;
            }

            var alicePrivateKey = new PrivateKey( Converter.HexToBytes( key ) );
            var aliceKeyPair = new KeyPair( alicePrivateKey );
            AlicePublicKey = aliceKeyPair.PublicKey;
            AliceAddress = SymbolCommonManager.Facade.Network.PublicKeyToAddress( AlicePublicKey );

            UpdateAccountDataAsync().Forget();

            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Load Key : Address = {AliceAddress.ToString()}" );
        }

        public string LoadPrivateKey()
        {
            return LoadText( GetSecureDataPath(), FileName );
        }

        public void GenerateAccount()
        {
            if(SymbolCommonManager.Facade == null)
            {
                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Facade null" );
                return;
            }
            if(AliceAddress != null)
            {
                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}AliceAddress not null" );
                return;
            }

            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}AliceAddress GenerateNewKeyPair" );
            var aliceKeyPair = KeyPair.GenerateNewKeyPair();
            var alicePrivateKey = aliceKeyPair.PrivateKey;
            AlicePublicKey = aliceKeyPair.PublicKey;
            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}AliceAddress PublicKeyToAddress" );
            AliceAddress = SymbolCommonManager.Facade.Network.PublicKeyToAddress( AlicePublicKey );

            SaveText(
                GetSecureDataPath(),
                FileName,
                Converter.BytesToHex( alicePrivateKey.bytes )
            );

            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Generate Key" );
        }

        public void DeleteAccount()
        {
            AlicePublicKey = null;
            AliceAddress = null;
            AliceAccountDatum = null;
            AliceXYM = 0;

            DeleteText( GetSecureDataPath(), FileName );

            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}DeleteAccount" );
        }

        public async UniTask<int> UpdateAccountDataAsync()
        {
            if(AliceAddress == null) return 1;
            var node = SymbolCommonManager.GetNode();
            Debug.Log( $"URL : " + node + $"/accounts/{AliceAddress}" );
            AliceAccountDatum = JsonUtility.FromJson<AccountDatum>( await SymbolApi.GetDataFromApi( node, $"/accounts/{AliceAddress}" ) );
            if(AliceAccountDatum == null) return 1;
            if(AliceAccountDatum.account == null) return 1;
            var result = AliceAccountDatum.account.mosaics.Find( n => n.id.Equals( SymbolCommonManager.XymId ) );
            if(result != null)
            {
                AliceXYM = double.Parse( result.amount );
            }
            return 0;
        }

        private string GetSecureDataPath()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            using(var unityPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" ))
            using(var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>( "currentActivity" ))
            using(var getFilesDir = currentActivity.Call<AndroidJavaObject>( "getFilesDir" ))
            {
                string secureDataPathForAndroid = getFilesDir.Call<string>( "getCanonicalPath" );
                return secureDataPathForAndroid;
            }
#else
            // TODO: 本来は各プラットフォームに対応した処理が必要
            return Application.persistentDataPath;
#endif
        }

        private void SaveText( string filePath, string fileName, string textToSave )
        {
#if UNITY_WEBGL
            PlayerPrefs.SetString( fileName, textToSave );
            PlayerPrefs.Save();
#else
            PlayerPrefs.SetString( fileName, textToSave );
            PlayerPrefs.Save();

            //var combinedPath = Path.Combine( filePath, fileName );
            //using( var streamWriter = new StreamWriter( combinedPath ) )
            //{
            //    streamWriter.WriteLine( textToSave );
            //}
#endif
        }

        public string LoadText( string filePath, string fileName )
        {
#if UNITY_WEBGL
            return PlayerPrefs.GetString( fileName );
#else
            return PlayerPrefs.GetString( fileName );

            //var combinedPath = Path.Combine( filePath, fileName );
            //if( !File.Exists( combinedPath ) )
            //{
            //    Debug.Log( $"Not File Exist : {fileName}" );
            //    return "";
            //}
            //
            //using( var streamReader = new StreamReader( combinedPath ) )
            //{
            //    return streamReader.ReadLine();
            //}
#endif
        }

        private void DeleteText( string filePath, string fileName )
        {
#if UNITY_WEBGL
            PlayerPrefs.DeleteKey( fileName );
            PlayerPrefs.Save();
#else
            PlayerPrefs.DeleteKey( fileName );
            PlayerPrefs.Save();
#endif
        }

    }
}

