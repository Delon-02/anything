using System;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public class FirebaseRegisterTest : MonoBehaviour
{
    private async void Start()
    {
        try
        {
            await FirebaseApp.CheckAndFixDependenciesAsync();

            FirebaseAuth auth = FirebaseAuth.DefaultInstance;

            string email = "testbakery123@gmail.com";
            string password = "Bakery123!";

            AuthResult result =
                await auth.CreateUserWithEmailAndPasswordAsync(
                    email, password
                );

            Debug.Log("REGISTER BERHASIL!");
            Debug.Log("UID: " + result.User.UserId);
        }
        catch (Exception ex)
        {
            Debug.LogError("REGISTER GAGAL: " + ex);
        }
    }
}