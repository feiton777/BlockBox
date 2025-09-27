using Cysharp.Threading.Tasks;
using SymbolEntity.Metadata;
using SymbolSdk.Symbol;
using SymbolSdk;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.InputSystem;
using System.IO;
using System.Security.Cryptography;
using System.Collections;
using System.Runtime.InteropServices;

namespace SB
{
    public class SymbolMosaicManager : MonoBehaviour
    {
        [DllImport( "__Internal" )]
        private static extern void DownloadDataFile( string fileName, byte[] data, int dataLength );


        public static SymbolMosaicManager Instance = null;

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

        // Start is called before the first frame update
        void Start()
        {

        }

        public async UniTask<int> CreateMosaicAsync( string filename, byte[] originalValueBytes )
        {
            // �閧���ǂݍ���
            var key = SymbolAccountManager.Instance.LoadPrivateKey();
            if(key == "")
            {
                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not load Key." );
                return 1;
            }
            var alicePrivateKey = new PrivateKey( Converter.HexToBytes( key ) );
            var aliceKeyPair = new KeyPair( alicePrivateKey );

            // ����XYM�m�F
            ulong costXym = (ulong)(Math.Truncate( originalValueBytes.Length / 1000000.0f ) + 1) * 1000 * 1000000;
            if(SymbolAccountManager.Instance.AliceXYM < costXym)
            {
                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}You don't have enough XYM. It will cost {costXym / 1000000}XYM." );
                return 1;
            }

            // �m�[�h�擾
            var node = SymbolCommonManager.GetNode();

            // ���U�C�N��`
            var nonce = BitConverter.ToUInt32( Crypto.RandomBytes( 8 ), 0 );
            var mosaicId = IdGenerator.GenerateMosaicId( SymbolAccountManager.Instance.AliceAddress, nonce );
            var newMosaicId = new UnresolvedMosaicId( mosaicId.mosaicId );
            var encryptSaltKey = newMosaicId.ToString().Substring( 2 );

            {
                var supplyMutable = false; //�����ʕύX�̉�
                var transferable = true;   //��O�҂ւ̏��n��
                var restrictable = false;  //�����ݒ�̉�
                var revokable = true;      //���s�҂���̊Ҏ���

                // ���U�C�N��`
                var mosaicDefTx = new EmbeddedMosaicDefinitionTransactionV1()
                {
                    Network = SymbolCommonManager.UseNetworkType,
                    Nonce = new MosaicNonce( nonce ),
                    SignerPublicKey = SymbolAccountManager.Instance.AlicePublicKey,
                    Id = new MosaicId( mosaicId.mosaicId ),
                    Duration = new BlockDuration( 0 ),
                    Divisibility = 0,
                    Flags = new MosaicFlags( Converter.CreateMosaicFlags( supplyMutable, transferable, restrictable, revokable ) ),
                };

                // ���U�C�N�ύX
                var mosaicChangeTx = new EmbeddedMosaicSupplyChangeTransactionV1()
                {
                    Network = SymbolCommonManager.UseNetworkType,
                    SignerPublicKey = SymbolAccountManager.Instance.AlicePublicKey,
                    MosaicId = new UnresolvedMosaicId( mosaicId.mosaicId ),
                    Action = MosaicSupplyChangeAction.INCREASE,
                    Delta = new Amount( 1000000 ),
                };

                // �g�����U�N�V������Z�߂�
                var innerTransactions = new IBaseTransaction[] { mosaicDefTx, mosaicChangeTx };
                var merkleHash = SymbolFacade.HashEmbeddedTransactions( innerTransactions );
                var aggregateTx = new AggregateCompleteTransactionV3()
                {
                    Network = SymbolCommonManager.UseNetworkType,
                    SignerPublicKey = SymbolAccountManager.Instance.AlicePublicKey,
                    Deadline = new Timestamp( SymbolCommonManager.Facade.Network.FromDatetime( DateTime.UtcNow ).AddHours( 2 ).Timestamp ),
                    Transactions = innerTransactions,
                    TransactionsHash = merkleHash,
                };
                TransactionHelper.SetMaxFee( aggregateTx, 100 );

                // ���M
                var signature = SymbolCommonManager.Facade.SignTransaction( aliceKeyPair, aggregateTx );
                var payload = TransactionHelper.AttachSignature( aggregateTx, signature );
                var hash = SymbolCommonManager.Facade.HashTransaction( aggregateTx, signature );
                var result = await SymbolApi.Announce( node, payload );

                if(result == null || result.Contains( "Uncaught Error" ))
                {
                    Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Announce Error ; {result}" );
                    return 1;
                }
            }

            // �V�K�쐬�������U�C�N���ǉ������̑҂��B�A�J�E���g�̏��X�V��SymbolAccountManager��Update�ɔC����
            while(true)
            {
                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}wait generate newMosaic : Id : {newMosaicId.ToString()} : {DateTime.Now.ToString()}" );

                //await UniTask.Delay( 5 * 1000 );
                //await SymbolAccountManager.Instance.UpdateAccountDataAsync();

                if(SymbolAccountManager.Instance != null &&
                    SymbolAccountManager.Instance.AliceAccountDatum != null &&
                    SymbolAccountManager.Instance.AliceAccountDatum.account != null &&
                    SymbolAccountManager.Instance.AliceAccountDatum.account.mosaics != null &&
                    0 < SymbolAccountManager.Instance.AliceAccountDatum.account.mosaics.Count)
                {
                    if(SymbolAccountManager.Instance.AliceAccountDatum.account.mosaics.Find( n => newMosaicId.ToString().Contains( n.id ) ) != null)
                    {
                        break;
                    }
                }
                //await UniTask.Delay( 5 * 1000 );
                await UniTask.DelayFrame( 1 );
            }
            //await UniTask.Delay(10 * 1000);

            // ���^�f�[�^�o�^
            {
                var dataSize = 0;

                // �f�[�^�Í���
                byte[] encryptedData = EncryptManager.Encrypt( originalValueBytes, key, encryptSaltKey );

                // �t�@�C�����o�^
                {
                    byte[] encryptedFileName = EncryptManager.Encrypt( Encoding.GetEncoding( "UTF-8" ).GetBytes( filename ), key, encryptSaltKey );

                    var innerTransactionList = new List<IBaseTransaction>();
                    var mosaicKey = IdGenerator.GenerateUlongKey( $"key_mosaic_name" );
                    var tx = new EmbeddedMosaicMetadataTransactionV1()
                    {
                        Network = SymbolCommonManager.UseNetworkType,
                        SignerPublicKey = SymbolAccountManager.Instance.AlicePublicKey,
                        TargetAddress = new UnresolvedAddress( SymbolAccountManager.Instance.AliceAddress.bytes ), //���U�C�N�쐬�҃A�h���X
                        TargetMosaicId = new UnresolvedMosaicId( mosaicId.mosaicId ), // mosaic id
                        ScopedMetadataKey = mosaicKey, // Key
                        Value = encryptedFileName, // Value
                        ValueSizeDelta = (ushort)encryptedFileName.Length
                    };
                    innerTransactionList.Add( tx );

                    var innerTransactions = innerTransactionList.ToArray();
                    var merkleHash = SymbolFacade.HashEmbeddedTransactions( innerTransactions );
                    var aggregateTx = new AggregateCompleteTransactionV3()
                    {
                        Network = SymbolCommonManager.UseNetworkType,
                        SignerPublicKey = SymbolAccountManager.Instance.AlicePublicKey,
                        Transactions = innerTransactions,
                        TransactionsHash = merkleHash,
                        Deadline = new Timestamp( SymbolCommonManager.Facade.Network.FromDatetime( DateTime.UtcNow ).AddHours( 2 ).Timestamp )
                    };
                    TransactionHelper.SetMaxFee( aggregateTx, 100 );
                    var signature = SymbolCommonManager.Facade.SignTransaction( aliceKeyPair, aggregateTx );
                    var payload = TransactionHelper.AttachSignature( aggregateTx, signature );
                    var result = await SymbolApi.Announce( node, payload );

                    Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Creating Mosaic : {result}" );

                    await UniTask.Delay( 100 );
                }

                var index = 1;
                while(dataSize < originalValueBytes.Length)
                {
                    var innerTransactionList = new List<IBaseTransaction>();

                    while(dataSize < encryptedData.Length && innerTransactionList.Count < 100)
                    {
                        var setDataSize = Math.Min( encryptedData.Length - dataSize, 1024 );
                        var setDataBytes = new byte[ setDataSize ];
                        Array.Copy( encryptedData, dataSize, setDataBytes, 0, setDataSize );

                        var mosaicKey = IdGenerator.GenerateUlongKey( $"key_mosaic_{index}" );
                        var tx = new EmbeddedMosaicMetadataTransactionV1()
                        {
                            Network = SymbolCommonManager.UseNetworkType,
                            SignerPublicKey = SymbolAccountManager.Instance.AlicePublicKey,
                            TargetAddress = new UnresolvedAddress( SymbolAccountManager.Instance.AliceAddress.bytes ), //���U�C�N�쐬�҃A�h���X
                            TargetMosaicId = new UnresolvedMosaicId( mosaicId.mosaicId ), // mosaic id
                            ScopedMetadataKey = mosaicKey, // Key
                            Value = setDataBytes, // Value
                            ValueSizeDelta = (ushort)setDataSize
                        };
                        innerTransactionList.Add( tx );
                        dataSize += setDataSize;
                        index++;
                    }

                    var innerTransactions = innerTransactionList.ToArray();
                    var merkleHash = SymbolFacade.HashEmbeddedTransactions( innerTransactions );
                    var aggregateTx = new AggregateCompleteTransactionV3()
                    {
                        Network = SymbolCommonManager.UseNetworkType,
                        SignerPublicKey = SymbolAccountManager.Instance.AlicePublicKey,
                        Transactions = innerTransactions,
                        TransactionsHash = merkleHash,
                        Deadline = new Timestamp( SymbolCommonManager.Facade.Network.FromDatetime( DateTime.UtcNow ).AddHours( 2 ).Timestamp )
                    };
                    TransactionHelper.SetMaxFee( aggregateTx, 100 );
                    var signature = SymbolCommonManager.Facade.SignTransaction( aliceKeyPair, aggregateTx );
                    var payload = TransactionHelper.AttachSignature( aggregateTx, signature );
                    var result = await SymbolApi.Announce( node, payload );

                    Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Creating Mosaic : {result}" );

                    await UniTask.Delay( 100 );
                }
            }

            // �g�p�����M
            {
                double tmpCostXym = costXym / 1000000;
                ulong sendCostXym = (ulong)Math.Round( tmpCostXym * 0.8f, 0, MidpointRounding.AwayFromZero ) * 1000000;
                await SymbolTransactionManager.SendTransactionAsync( $"Write Mosaic Meta : {newMosaicId.ToString()} : {DateTime.Now.ToString()}", $"{SymbolCommonManager.RecieveAccount}", SymbolCommonManager.XymId, sendCostXym, node );
            }

            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Mosaic Create End : ID : {newMosaicId.ToString()}" );
            return 0;
        }

        public async UniTask<int> LoadMosaicAsync( string mosaicId, string outputFilePath )
        {
            string filePath = UnityEngine.Application.persistentDataPath + "/" + mosaicId;

            // �m�[�h�擾
            var node = SymbolCommonManager.GetNode();

            var data = "";

            // �f�[�^�ǂݍ���
            var metaDatasList = new List<MetadataDatum>();
            int pageIndex = 1;
            bool endFlag = false;
            while(true)
            {
                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Loading Mosaic Metadata." );

                async UniTask<int> LoadDataAsync( int loadIndex )
                {
                    var metadatas = JsonUtility.FromJson<MetadataRoot>( await SymbolApi.GetDataFromApi( node, $"/metadata?targetId={mosaicId}&metadataType=1&pageSize=100&pageNumber={loadIndex}" ) );
                    if(metadatas.data.Count() <= 0)
                    {
                        endFlag = true;
                        return 0;
                    }
                    metaDatasList.AddRange( metadatas.data );
                    return 0;
                }

                var loadAsyncList = new List<UniTask<int>>();
                for(int i = 0; i < 10; i++)
                {
                    loadAsyncList.Add( LoadDataAsync( pageIndex ) );
                    pageIndex++;
                }
                await UniTask.WhenAll( loadAsyncList );

                if(endFlag) break;
            }

            // key��index���Ɍ���
            int dataNum = metaDatasList.Count;
            for(int i = 0; i < dataNum; i++)
            {
                var mosaicKey = IdGenerator.GenerateUlongKey( $"key_mosaic_{i + 1}" ).ToString( "X" );
                var result = metaDatasList.Find( m => m.metadataEntry.scopedMetadataKey.Equals( mosaicKey ) );
                if(result == null) continue;
                data += result.metadataEntry.value;
                metaDatasList.Remove( result );
                if(i % 10 == 0)
                {
                    await UniTask.Yield( PlayerLoopTiming.Update );
                }
            }

            if(data == "")
            {
                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not load data.Check Mosaic ID." );
                return 1;
            }

            byte[] bytes = Converter.HexToBytes( data );

            // �閧���ǂݍ���
            var key = SymbolAccountManager.Instance.LoadPrivateKey();
            if(key == "")
            {
                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Could not load Key." );
                return 1;
            }

            // �f�[�^������
            byte[] decryptedData = EncryptManager.Decrypt( bytes, key, mosaicId );

            // �t�@�C����
            string filename = "";
            {
                for(int i = 0; i < dataNum; i++)
                {
                    var mosaicKey = IdGenerator.GenerateUlongKey( $"key_mosaic_name" ).ToString( "X" );
                    var result = metaDatasList.Find( m => m.metadataEntry.scopedMetadataKey.Equals( mosaicKey ) );
                    if(result == null) continue;

                    byte[] filenameBytes = Converter.HexToBytes( result.metadataEntry.value );
                    byte[] decryptedFileName = EncryptManager.Decrypt( filenameBytes, key, mosaicId );
                    filename = Encoding.GetEncoding( "UTF-8" ).GetString( decryptedFileName );
                    break;
                }
            }

            Debug.Log( $"{SymbolCommonManager.SymbolLogKey}decryptedData : {filename} : {decryptedData.Length}" );

            // �f�[�^�ۑ�
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                //// byte[]��Base64������ɕϊ��B
                //// byte[]�����̂܂ܓn���Ɠn���ۂ�Unity������Base64�ɏ���ɕϊ�����đz��Ƌ������ς�邽�ߎ��O�ɕϊ��B
                //string base64Data = Convert.ToBase64String( decryptedData );
                //
                //// JavaScript�̊֐����Ăяo���A�t�@�C������Base64�������n��
                //DownloadDataFile( filename, base64Data );

                DownloadDataFile( filename, decryptedData, decryptedData.Length );
#else
                //var url = await FileDialogManager.Instance.OpenFolderAsync();
                //if(url == null || url == "") return 1;

                File.WriteAllBytes( $"{outputFilePath}/{filename}", decryptedData );
#endif

                Debug.Log( $"{SymbolCommonManager.SymbolLogKey}Load Mosaic Done." );
            }

            return 0;
        }

    }
}

