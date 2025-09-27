using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SymbolEntity.Account;
using SymbolSdk.Symbol;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SB
{
    public static class GenerateNodeListEditor
    {
        //public static SymbolFacade Facade { get; } = new SymbolFacade( Network.TestNet );
        //public const string Node = "https://001-sai-dual.symboltest.net:3001";
        //public const string Node = "https://testnet1.symbol-mikun.net:3001"; // "https://test01.xymnodes.com:3001";

        [MenuItem( "Tools/Symbol/GenerateNodeList/TestNet" )]
        public static void GenerateTestNetNodeList()
        {
            GenerateAsync( "https://testnet1.symbol-mikun.net:3001", "TestNetNodeList.txt" ).Forget();
        }
        [MenuItem( "Tools/Symbol/GenerateNodeList/MainNet" )]
        public static void GenerateMainNetNodeList()
        {
            GenerateAsync( "https://0-0-xym.cubkab-crypto.tokyo:3001", "MainNetNodeList.txt" ).Forget();
        }

        private static bool _isGenerating = false;
        public static async UniTask<int> GenerateAsync( string baseNode, string outputFileName )
        {

            if(_isGenerating) return 1;
            _isGenerating = true;

            var nodeList = new List<string>();
            nodeList.Add( baseNode );
            var removeNodeList = new List<string>();

            string nodeListTxtPath = $"{Application.streamingAssetsPath}/Symbol/{outputFileName}";
            if(File.Exists( nodeListTxtPath ))
            {
                var readDatas = File.ReadAllLines( nodeListTxtPath );
                if(0 < readDatas.Length)
                {
                    nodeList.Clear();
                    nodeList.AddRange( readDatas );
                }
            }
            else
            {
                File.Create( nodeListTxtPath );
            }

            for(int index = 0; index < nodeList.Count; index++)
            {
                var responseData = await SymbolApi.GetDataFromApi( nodeList[ index ], $"/node/peers" );
                if(responseData == null)
                {
                    removeNodeList.Add( nodeList[ index ] );
                    continue;
                }
                var transactionData = JsonNode.Parse( responseData );

                for(int i = 0; i < transactionData.Count; i++)
                {
                    var roles = transactionData[ i ][ "roles" ].Get<long>();
                    if((roles & 2) != 0)
                    {
                        var host = $"https://{transactionData[ i ][ "host" ].Get<string>()}:3001";

                        if(nodeList.Contains( host )) continue;

                        nodeList.Add( host );
                        Debug.Log( $"{host}" );
                    }
                }
            }

            for(int i = 0; i < removeNodeList.Count; i++)
            {
                if(!nodeList.Contains( removeNodeList[ i ] )) continue;
                nodeList.Remove( removeNodeList[ i ] );
            }

            {
                File.WriteAllLines( nodeListTxtPath, nodeList );
            }

            Debug.Log( $"Generate End" );
            _isGenerating = false;
            return 0;


            /*
            if(_isGenerating) return 1;
            _isGenerating = true;

            var nodeList = new List<string>();
            nodeList.Add( "https://testnet1.symbol-mikun.net:3001" );

            for(int index = 0; index < nodeList.Count; index++)
            {
                var responseData = await SymbolApi.GetDataFromApi( nodeList[index], $"/node/peers" );
                if(responseData == null)
                {
                    continue;
                }
                var transactionData = JsonNode.Parse( responseData );

                for(int i = 0; i < transactionData.Count; i++)
                {
                    var roles = transactionData[ i ][ "roles" ].Get<long>();
                    if((roles & 2) != 0)
                    {
                        var host = $"https://{transactionData[ i ][ "host" ].Get<string>()}:3001";

                        if(nodeList.Contains( host )) continue;

                        nodeList.Add( host );
                        Debug.Log( $"{host}" );
                    }
                }
            }

            Debug.Log( $"Generate End" );
            _isGenerating = false;
            return 0;
            */
        }

    }
}

