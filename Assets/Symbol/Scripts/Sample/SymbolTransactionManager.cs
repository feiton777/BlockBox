using Cysharp.Threading.Tasks;
using SymbolSdk.Symbol;
using SymbolSdk;
using System.Net.Http;
using System.Text;
using System;
using UnityEngine;
using SymbolEntity.Network;
using System.Globalization;
using System.Numerics;
using SymbolEntity.Account;
using MiniJSON;

namespace SB
{
    public class SymbolTransactionManager : MonoBehaviour
    {
        public static SymbolTransactionManager Instance = null;

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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public static async UniTask<int> SendTransactionAsync( string message, string targetAddress, string sendMosaicId, ulong sendMosaicNum, string selectNode = null ) //SetFlagAsync
        {
            // 秘密鍵読み込み
            var key = SymbolAccountManager.Instance.LoadPrivateKey();
            if(key == "")
            {
                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not load Key." );
                return 1;
            }
            var alicePrivateKey = new PrivateKey( Converter.HexToBytes( key ) );
            var aliceKeyPair = new KeyPair( alicePrivateKey );

            // ノード取得
            var node = SymbolCommonManager.GetNode();
            if(selectNode != null)
            {
                node = selectNode;
            }

            UnresolvedAddress bobAddress = new UnresolvedAddress( Base32Converter.FromBase32String( targetAddress ) );

            ulong mosaicId = Convert.ToUInt64( sendMosaicId, 16 );

            var tx = new TransferTransactionV1
            {
                Network = SymbolCommonManager.UseNetworkType,
                RecipientAddress = new UnresolvedAddress( bobAddress.bytes ),
                SignerPublicKey = SymbolAccountManager.Instance.AlicePublicKey,
                Mosaics = new UnresolvedMosaic[]
                {
                new ()
                {
                    MosaicId = new UnresolvedMosaicId(mosaicId), // UnresolvedMosaicId(0x72C0212E67A08BCE)
                    Amount = new Amount(sendMosaicNum) //1XYM(divisibility:6)
                }
                },
                Message = Converter.Utf8ToPlainMessage( message ), //メッセージ
                Deadline = new Timestamp( SymbolCommonManager.Facade.Network.FromDatetime( DateTime.UtcNow ).AddHours( 2 ).Timestamp ) //Deadline:有効期限
            };
            tx.Sort();
            TransactionHelper.SetMaxFee( tx, 100 ); //手数料

            var signature = SymbolCommonManager.Facade.SignTransaction( aliceKeyPair, tx );
            var payload = TransactionHelper.AttachSignature( tx, signature );
            var hash = SymbolCommonManager.Facade.HashTransaction( tx, signature );
            var result = await SymbolApi.Announce( node, payload );

            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}SendTransaction : {result}" );

            return 0;
        }

        public static async UniTask<int> CheckRecipientTransactionAsync( string recipientAddress )
        {
            if(SymbolAccountManager.Instance.AliceAddress == null)
            {
                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not get Address." );
                return 1;
            }
            var node = SymbolCommonManager.GetNode();
            Debug.Log( $"URL : " + node + $"/transactions/confirmed?recipientAddress={recipientAddress}&order=desc" );
            var result = await SymbolApi.GetDataFromApi( node, $"/transactions/confirmed?recipientAddress={recipientAddress}&order=desc" );
            if(result == "" || result == null)
            {
                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}RecipientTransaction : failed" );
                return 1;
            }
            else
            {
                //Debug.Log( $"{SymbolCommonManager.SymbolLogKey}RecipientTransaction : {result}" );
            }

            var resultJson = JsonNode.Parse( result );

            var transactionData = JsonNode.Parse( result )[ "data" ];
            if(transactionData == null)
            {
                return 1;
            }

            for(int i = transactionData.Count - 1; 0 <= i; i--)
            {
                if(transactionData[ i ][ "transaction" ][ "message" ] == null) continue;
                var messageData = transactionData[ i ][ "transaction" ][ "message" ].Get<string>();
                if(messageData == null)
                {
                    continue;
                }
                if(messageData.Length <= 2)
                {
                    continue;
                }
                messageData = messageData.Substring( 2 );

                byte[] HexStringToByte( string message )
                {
                    byte[] byteArray = new byte[ message.Length / 2 ];

                    for(int i = 0; i < message.Length; i += 2)
                    {
                        // 2文字ずつを取り出し、数値に変換してbyte配列に格納
                        byteArray[ i / 2 ] = byte.Parse( message.Substring( i, 2 ), System.Globalization.NumberStyles.HexNumber );
                    }
                    return byteArray;
                }
                var messageByte = HexStringToByte( messageData );
                messageData = System.Text.Encoding.UTF8.GetString( messageByte );

                var hashData = transactionData[ i ][ "meta" ][ "hash" ].Get<string>();

                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}RecipientTransaction : {messageData}" );
            }


            return 0;
        }
    }
}

