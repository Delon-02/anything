using System;
using System.Threading.Tasks;
using Firebase;
using UnityEngine;

public class FirebaseConnectionTest : MonoBehaviour
{
    private async void Start()
    {
        Debug.Log("Memulai pemeriksaan Firebase...");

        try
        {
            DependencyStatus status =
                await FirebaseApp.CheckAndFixDependenciesAsync();

            if (status == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;

                Debug.Log("FIREBASE BERHASIL DIINISIALISASI!");
                Debug.Log("Firebase App: " + app.Name);
            }
            else
            {
                Debug.LogError(
                    "Firebase dependency bermasalah: " + status
                );
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Firebase initialization error: " + ex
            );
        }
    }
}