using NUnit.Framework;
using NUnit.Framework.Interfaces;
using SymbolSdk.Symbol;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static System.Net.WebRequestMethods;
using Cysharp.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System;
using UnityEngine.Networking;
using UnityEditor;


namespace SB
{
    public class SymbolCommonManager : MonoBehaviour
    {
        static public Network UseNetwork = Network.TestNet; // Network.TestNet Network.MainNet
        static public NetworkType UseNetworkType
        {
            get
            {
                if(UseNetwork == Network.TestNet)
                {
                    return NetworkType.TESTNET;
                }
                return NetworkType.MAINNET;
            }
        }

        static public SymbolFacade Facade { get; } = new SymbolFacade( UseNetwork );
        static public string SymbolLogKey = "SymbolLog:";
        static public string XymId
        {
            get
            {
                if(UseNetwork == Network.MainNet)
                {
                    return "6BED913FA20223F8";
                }
                return "72C0212E67A08BCE";
            }
        }
        static public string RecieveAccount
        {
            get
            {
                if(UseNetwork == Network.MainNet)
                {
                    return "NAIE5WGWY6SHTYMM3ZCTS5TGM2BXOVLXGPMN3LY";
                }
                return "TBYGCI5P7MLSQ7QGGQMXKGS636RSDDZRPTW2MEA";
            }
        }

        static List<string> TestNetNodes = new List<string>();
        static List<string> MainNetNodes = new List<string>();


        void Start()
        {
            Init().Forget();
        }

        static async UniTask<int> Init()
        {
            // TestNetのノードリスト取得
            string nodeListTxtPath = $"{Application.streamingAssetsPath}/Symbol/TestNetNodeList.txt";

            using(UnityWebRequest request = UnityWebRequest.Get( nodeListTxtPath ))
            {
                var operation = request.SendWebRequest();
                await operation;

                if(request.result == UnityWebRequest.Result.Success)
                {
                    if(request.downloadHandler.text == null || request.downloadHandler.text.Length <= 0)
                    {
                        //Debug.Log( $"{SymbolCommonManager.SymbolLogKey}No Data" );
                    }
                    else
                    {
                        var readDatas = request.downloadHandler.text.Split( "\r\n" );
                        if(0 < readDatas.Length)
                        {
                            TestNetNodes.Clear();
                            TestNetNodes.AddRange( readDatas );
                        }
                        Debug.Log( "StreamingAssetsから読み込んだテキスト: " + request.downloadHandler.text );
                    }
                }
                else
                {
                    Debug.LogError( $"An error has occurred.: {request.error}" );
                }
            }

            // MainNetのノードリスト取得
            nodeListTxtPath = $"{Application.streamingAssetsPath}/Symbol/MainNetNodeList.txt";

            using(UnityWebRequest request = UnityWebRequest.Get( nodeListTxtPath ))
            {
                var operation = request.SendWebRequest();
                await operation;

                if(request.result == UnityWebRequest.Result.Success)
                {
                    if(request.downloadHandler.text == null || request.downloadHandler.text.Length <= 0)
                    {
                        //Debug.Log( $"{SymbolCommonManager.SymbolLogKey}No Data" );
                    }
                    var readDatas = request.downloadHandler.text.Split( "\r\n" );
                    if(0 < readDatas.Length)
                    {
                        MainNetNodes.Clear();
                        MainNetNodes.AddRange( readDatas );
                    }
                }
                else
                {
                    Debug.LogError( $"An error has occurred.: {request.error}" );
                }
            }

            // 現在時刻のミリ秒でシード値を初期化
            UnityEngine.Random.InitState( DateTime.Now.Millisecond );

            return 0;
        }

        // NodeListからランダムでNode取得
        static public string GetNode()
        {
            var nodes = TestNetNodes;
            if(UseNetwork == Network.TestNet)
            {
                if(TestNetNodes.Count <= 0) return "https://test01.xymnodes.com:3001";
                nodes = TestNetNodes;
            }
            else if(UseNetwork == Network.MainNet)
            {
                if(MainNetNodes.Count <= 0) return "https://0-0-xym.cubkab-crypto.tokyo:3001";
                nodes = MainNetNodes;
            }
            return nodes[ UnityEngine.Random.Range( 0, nodes.Count ) ];
        }
    }
}
