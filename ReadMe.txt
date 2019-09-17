Use this project to stream the live feed of HoloLens's PoV camera to your system.
UPDATE 3: Please don't use Unity 2017.3.x as networking for UWP is broken in those version. This is a known issue (https://github.com/Microsoft/MixedRealityToolkit-Unity/issues/1658). I would recommend not using 2017.4.0x as well.
UPDATE 2: Real-time data transfer has been implemented which should transfer the frames with almost real-time level lag and latency.
UPDATE 1: Here is a demo
https://www.youtube.com/watch?v=SWhR5Ve9z-w

TCP based PC Receiver can be found in "Packages" directory.

STEPS:
1. Both HoloLens and Unity App should be on the same network
2. Structure
 a) HoloLens is a client that looks for a receiving server
 b) Enter the IP address and Port Number of the receiving server (which can be another Unity app)
 c) REMOVE ANY FIREWALL RESTRICTIONS - allow Unity to send and receive messages

Allow Unity Communication through Firewall
1. Do a ping test first. 
Example: ping 192.168.137.1

2. If fails, for the Sender station/PC, go to
Control Panel\Network and Internet\Network and Sharing Center\Advanced sharing settings
"Turn on Network Discovery" with 
"Turn on automatic setup of network connected devices"
"Turn on File and Printer Sharing"

3. First, try disabling Firewall and test if it works. Then, check if it works for the following settings:
Firewall Off for Private | Off for Public on sender
Firewall Off for Private | On for Public on sender
Firewall On for Private | On for Public on sender
This will help in narrowing down what rules to add or delete in Step 4

3. Allow Unity Editor through Firewall
 a) Go to Control Panel\System and Security\Windows Defender Firewall
 b) Click on "Allow and app or feature through Firewall"
 c) Click on "Change Settings" and check "Unity Editor" for "Private" (Do Public only if you are sure)
 d) Go to advanced settings for Windows Firewall
 e) Go to "Inbound rules"
 f) There will 1 or 2 rules for Unity Editor and most probably, they will be "Enabled" and will be set to "Block"
 g) Disable the rule that blocks Unity Editor for Private Connections

References

1. UDP Communication
 - https://forums.hololens.com/discussion/7980/udp-communication-solved#latest
2. Problems with async
 - https://social.msdn.microsoft.com/Forums/en-US/f2b768bc-c999-44b1-b539-bfba4ab3969b/problem-with-asyc-method-cant-figure-out-how-to-solve-the-errors?forum=winappswithcsharp
 - https://stackoverflow.com/questions/34486638/socket-connectasync-for-windows-store-application-does-not-like-async
3. Windows StreamSocket Example (Windows Phone 8)
 - https://csharp.hotexamples.com/examples/Windows.Networking.Sockets/StreamSocket/Dispose/php-streamsocket-dispose-method-examples.html
4. TCP Client in a UWP APP on HoloLens (Most Important)
 - https://foxypanda.me/tcp-client-in-a-uwp-unity-app-on-hololens/#thecode
5. Missing .NET APIs in UNITY and UWP
 - https://docs.microsoft.com/en-us/windows/uwp/gaming/missing-dot-net-apis-in-unity-and-uwp
6. Asynchronous REST API web calls from HoloLens
 - http://www.appzinside.com/2017/03/28/asynchronous-rest-api-web-calls-from-hololens-apps-using-unity-and-visual-studio/
7. Windows 10, UWP, HoloLens & A Simple Two-Way Socket Library
 - https://mtaulty.com/2017/02/21/windows-10-uwp-hololens-a-simple-two-way-socket-library/
8. Windows Namespaces
 - https://docs.microsoft.com/en-us/uwp/api/windows.system.threading
 - https://docs.microsoft.com/en-us/uwp/api/windows.networking

Other general resources
1. General hiccups
 - https://saraford.net/2017/03/10/developing-my-first-hololens-app/
2. 3D Test in HoloLens:
 - https://forums.hololens.com/discussion/963/how-can-i-display-hello-world-text-on-my-hologram-app
3. Setting up
 - https://pterneas.com/2016/04/04/getting-started-hololens-unity3d/
 - https://developer.microsoft.com/en-us/windows/mixed-reality/holograms_100
 - https://developer.microsoft.com/en-us/windows/mixed-reality/holograms_101
4. WebCam Streaming in Unity
 - https://stackoverflow.com/questions/42717713/unity-live-video-streaming (The best one)
 - https://forum.unity.com/threads/stream-video-through-network.464693/
 - https://stackoverflow.com/questions/47575438/convert-color32-array-to-byte-array-to-send-over-network?noredirect=1&lq=1
 - https://www.youtube.com/watch?v=qGkkaNkq8co
 - https://answers.unity.com/questions/1349462/unity-simple-client-server-video-streaming-using-r.html
 - https://www.youtube.com/watch?v=3cHT9unX8Ig
 - https://docs.unity3d.com/455/Documentation/ScriptReference/WWW.LoadImageIntoTexture.html
 - https://docs.unity3d.com/ScriptReference/Network.OnSerializeNetworkView.html
 - https://github.com/mrayy/UnityCam
 - https://gist.github.com/danielbierwirth/0636650b005834204cb19ef5ae6ccedb
5. Locatable camera in HoloLens (Did not use in this project, but could be used for optimization)
 - https://forum.unity.com/threads/locatable-camera-in-unity.398803/
 - https://forums.hololens.com/discussion/782/live-stream-of-locatable-camera-webcam-in-unity 
6. Adding MRDL (Mixed-Reality Design Lab_ to HoloLens Projects in Unity
 - https://www.youtube.com/watch?v=eNG336Wzp3s

Extra
1. Mixed-Reality VR Headset and Leap Motion
 - Windows Mixed Reality display with a Leap Motion
2. Face tracking sample for HoloLens
 - https://github.com/Microsoft/Windows-universal-samples/tree/master/Samples/HolographicFaceTracking
