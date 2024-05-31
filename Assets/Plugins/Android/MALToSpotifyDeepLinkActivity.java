package com.DefaultCompany.MALToSpotify;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class MALToSpotifyDeepLinkActivity extends UnityPlayerActivity
{
    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        handleIntent(getIntent());
    }

    @Override
    protected void onNewIntent(Intent intent)
    {
        super.onNewIntent(intent);
        setIntent(intent);
        handleIntent(intent);
    }

    private void handleIntent(Intent intent)
    {
        Uri uri = intent.getData();
        if (uri != null)
        {
            String scheme = uri.getScheme();
            String host = uri.getHost();
            String code = uri.getQueryParameter("code");

            if ("auth".equals(host))
            {
                if ("mal2spotify".equals(scheme))
                {
                    UnityPlayer.UnitySendMessage("MAL Controller", "CreateMALClient", code);
                }
                else if ("spotify2mal".equals(scheme))
                {
                    UnityPlayer.UnitySendMessage("MAL Controller", "CreateSpotifyClient", code);
                }
            }
        }
    }
}
