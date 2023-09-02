using System;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Events;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LWOS
{
	static class Extensions
	{
		//X Event called when Game Save Data
		public delegate void SaveData();
		public static SaveData onSaveData;
		
		public static void SaveGameData()
		{
			if (onSaveData != null)
				onSaveData();
		}
		
		//X Event called when Game Load Data
		public delegate void LoadData();
		public static LoadData onLoadData;
		
		public static void LoadGameData()
		{
			if (onLoadData != null)
				onLoadData();
		}
		
		//X Event called when new Faction or Pack GetSpawned
		public delegate void SetFactions(string faction, string level, bool isWild);
		public static SetFactions onPackSetFaction;
		
		public static void SetPackFaction(string faction, string level, bool isWild)
		{
			if (onPackSetFaction != null)
				onPackSetFaction(faction, level, isWild);
		}
		
		//X Set & Get Encrypted PlayerPrefs
		public static void SetPlayerData(string playerData, string playerValue)
		{
			string encryptedData = EncryptSTR(playerData);
			string encryptedValue = EncryptSTR(playerValue);

			PlayerPrefs.SetString(encryptedData, encryptedValue);
		}

		public static string GetPlayerString(string playerData)
		{
			string encryptedData = PlayerPrefs.GetString(EncryptSTR(playerData));
			string DecryptedValue = DecryptSTR(encryptedData);

			return DecryptedValue;
		}

		public static int GetPlayerInt(string playerData)
		{
			string encryptedData = PlayerPrefs.GetString(EncryptSTR(playerData));
			string DecryptedValue = DecryptSTR(encryptedData);
			int.TryParse(DecryptedValue, out int DecryptedValueReal);

			return DecryptedValueReal;
		}

		public static float GetPlayerFloat(string playerData)
		{
			string encryptedData = PlayerPrefs.GetString(EncryptSTR(playerData));
			string DecryptedValue = DecryptSTR(encryptedData);
			float.TryParse(DecryptedValue, out float DecryptedValueReal);
			
			return DecryptedValueReal;
		}

		public static bool GetPlayerBool(string playerData)
		{
			string encryptedData = PlayerPrefs.GetString(EncryptSTR(playerData));
			string DecryptedValue = DecryptSTR(encryptedData);
			int.TryParse(DecryptedValue, out int DecryptedValueReal);
			
			return DecryptedValueReal == 0 ? false : true;
		}

        //X Utility
		
		public static float Lerp(float value1, float value2, float by)
		{
            return value1 * (1f - by) + value2 * by;
        }

		public static Vector3 LimitToScreenPos(this Vector3 pos)
        {
            return new Vector3(pos.x > Screen.currentResolution.width ? Screen.currentResolution.width : pos.x, pos.y > Screen.currentResolution.height ? Screen.currentResolution.height : pos.y, 0);
        }

		public static bool Contains(this LayerMask mask, int layer)
		{
            return mask == (mask | (1 << layer));
        }

        //X Security & Encryption Part

        public static string GetMD5(this string text)
		{
			byte[] encodedPassword = new System.Text.UTF8Encoding().GetBytes(text);
			byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
			return System.BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
		}

        
		public static string EncryptSTR(string text)
        {
            var data = Encoding.UTF8.GetBytes(text);
                
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var keys = md5.ComputeHash(Encoding.UTF8.GetBytes(GameData.Instance.SecretKey));
                using (var tripDes = new TripleDESCryptoServiceProvider { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    var transform = tripDes.CreateEncryptor();
                    var results = transform.TransformFinalBlock(data, 0, data.Length);
                    return Convert.ToBase64String(results, 0, results.Length);
                }
            }
        }
    
        public static string DecryptSTR(string cipher)
        {
            var data = Convert.FromBase64String(cipher);
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var keys = md5.ComputeHash(Encoding.UTF8.GetBytes(GameData.Instance.SecretKey));
                using (var tripDes = new TripleDESCryptoServiceProvider { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                        var transform = tripDes.CreateDecryptor();
                        var results = transform.TransformFinalBlock(data, 0, data.Length);
                        return Encoding.UTF8.GetString(results);
                }
            }
        }

        public static byte[] Encrypt<T>(T obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            byte[] data;
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                data = stream.ToArray();
            }

            byte[] key = new MD5CryptoServiceProvider().ComputeHash(System.Text.Encoding.UTF8.GetBytes(GameData.Instance.SecretKey));
            using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider())
            {
                des.Key = key;
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7;

                using (MemoryStream encryptedStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(encryptedStream, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                        return encryptedStream.ToArray();
                    }
                }
            }
        }

        public static T Decrypt<T>(byte[] encryptedData)
        {
            byte[] key = new MD5CryptoServiceProvider().ComputeHash(System.Text.Encoding.UTF8.GetBytes(GameData.Instance.SecretKey));
            using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider())
            {
                des.Key = key;
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7;

                using (MemoryStream decryptedStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(decryptedStream, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(encryptedData, 0, encryptedData.Length);
                        cryptoStream.FlushFinalBlock();

                        decryptedStream.Position = 0;
                        BinaryFormatter formatter = new BinaryFormatter();
                        return (T)formatter.Deserialize(decryptedStream);
                    }
                }
            }
        }
    }
}
