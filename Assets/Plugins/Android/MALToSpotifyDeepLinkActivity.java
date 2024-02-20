package com.DefaultCompany.MALToSpotify;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class MALToSpotifyDeepLinkActivity extends UnityPlayerActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        // Handle the intent (e.g., get data from the URL) as needed
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        String action = intent.getAction();
        Uri data = intent.getData();
        if (Intent.ACTION_VIEW.equals(action) && data != null) {
            String scheme = data.getScheme();
            String host = data.getHost();
            // Use the data from the URL as needed
            String code = data.getQueryParameter("code");
            // if (code != null) {
            //     // Use the extracted code as needed
            //     // For example, log it or pass it to another activity
            //     //Log.d("MALToSpotifyDeepLinkActivity", "Received code: " + code);
            //     // Send the code back to Unity
            //     sendCodeToUnity(code);
            // }
            sendCodeToUnity(code);
        }
    }

    private void sendCodeToUnity(String code) {
        // Call a method in a Unity script to handle the received code
        // Replace "YourScriptName" with the actual name of your Unity script
        // Replace "HandleCode" with the name of the method in your Unity script that handles the code
        UnityPlayer.UnitySendMessage("MALAuthenticator", "HandleCode", code);
    }
}
