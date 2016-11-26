using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Java.Security;
using Javax.Crypto;
using Android.Hardware.Fingerprints;
using Android.Support.V4.App;
using Android;
using System;
using Android.Security.Keystore;

namespace XamarinFingerAuth
{
    [Activity(Label = "XamarinFingerAuth", MainLauncher = true, Icon = "@drawable/icon",Theme ="@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {
        private KeyStore keyStore;
        private Cipher cipher;
        private string KEY_NAME = "EDMTDev";
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView (Resource.Layout.Main);

            KeyguardManager keyguardManager = (KeyguardManager)GetSystemService(KeyguardService);
            FingerprintManager fingerprintManager = (FingerprintManager)GetSystemService(FingerprintService);

            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.UseFingerprint) != (int)Android.Content.PM.Permission.Granted)
                return;
            if (!fingerprintManager.IsHardwareDetected)
                Toast.MakeText(this, "Fingerprint authentication permission not enalbe", ToastLength.Long).Show();
            else
            {
                if(!fingerprintManager.HasEnrolledFingerprints)
                    Toast.MakeText(this, "Register at least one fingerprint in Settings", ToastLength.Long).Show();
                else
                {
                    if (!keyguardManager.IsKeyguardSecure)
                        Toast.MakeText(this, "Lock screen security not enable in Settings", ToastLength.Long).Show();
                    else
                        GenKey();
                    if(CipherInit())
                    {
                        FingerprintManager.CryptoObject cryptoObject = new FingerprintManager.CryptoObject(cipher);
                        FingerprintHandler helper = new FingerprintHandler(this);
                        helper.StartAuthentication(fingerprintManager, cryptoObject);
                    }
                }
            }


        }

        private bool CipherInit()
        {
           try
            {
                cipher = Cipher.GetInstance(KeyProperties.KeyAlgorithmAes
                    + "/"
                    + KeyProperties.BlockModeCbc
                    + "/"
                    + KeyProperties.EncryptionPaddingPkcs7);
                keyStore.Load(null);
                IKey key = (IKey)keyStore.GetKey(KEY_NAME, null);
                cipher.Init(CipherMode.EncryptMode, key);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        private void GenKey()
        {
            keyStore = KeyStore.GetInstance("AndroidKeyStore");
            KeyGenerator keyGenerator = null;
            keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, "AndroidKeyStore");
            keyStore.Load(null);
            keyGenerator.Init(new KeyGenParameterSpec.Builder(KEY_NAME, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                .SetBlockModes(KeyProperties.BlockModeCbc)
                .SetUserAuthenticationRequired(true)
                .SetEncryptionPaddings(KeyProperties.EncryptionPaddingPkcs7)
                .Build());
            keyGenerator.GenerateKey();
        }
    }
}

