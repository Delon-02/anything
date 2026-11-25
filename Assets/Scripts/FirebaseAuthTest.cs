using System;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public class FirebaseAuthTest : MonoBehaviour
{
    private async void Start()
    {
        Debug.Log("Memulai pengujian Firebase Auth...");

        try
        {
            DependencyStatus status =
                await FirebaseApp.CheckAndFixDependenciesAsync();

            if (status != DependencyStatus.Available)
            {
                Debug.LogError(
                    "Firebase dependency error: " + status
                );
                return;
            }

            FirebaseAuth auth = FirebaseAuth.DefaultInstance;

            Debug.Log("FIREBASE AUTH BERHASIL DIAKSES!");

            if (auth.CurrentUser == null)
            {
                Debug.Log("Belum ada pengguna yang login.");
            }
            else
            {
                Debug.Log("Pengguna aktif: " + auth.CurrentUser.UserId);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Firebase Auth Error: " + ex);
        }
    }
}