// See https://aka.ms/new-console-template for more information

using System;
using System.Security.Cryptography;

var rng = new RNGCryptoServiceProvider();
var keyBytes = new byte[32]; // 256-bit key for HMAC-SHA256
rng.GetBytes(keyBytes);
var key = Convert.ToBase64String(keyBytes);
Console.WriteLine(key); // Use this value as your secret key