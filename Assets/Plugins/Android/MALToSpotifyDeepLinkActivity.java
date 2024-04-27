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
        handleIntent(getIntent());
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent);
        handleIntent(intent);
    }

    private void handleIntent(Intent intent) {
        Uri uri = intent.getData();
        if (uri != null) {
            String code = uri.getQueryParameter("code");
            UnityPlayer.UnitySendMessage("MAL Controller", "CreateMALClient", code);    // The first field is the name of the script's GameObject, not the script's name
        }
    }
}
