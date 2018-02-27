# CafebazaarUnity
Cafebazaar In-app purchase Unity plugin


## BUILD INSTRUCTION
1. Open a command prompt
2. Navigate to JavaPlugin folder
3. Type `gradlew createJar`
4. After the build is succeeded you can find `BazaarIAB.jar` in the build folder


## INSIDE UNITY PROJECT
This plugin has not any prefab to use, it will manage the required objects.

There is `IABEventManager` class that you can subscribe to its events.


The `BazaarIAB` is the interface that let you call billing functions.
Befor calling any other function try to initialize the plugin by calling the `init` with the public key provided by Cafebazaar.


Add the plugin activity in the Application section of the `AndroidManifest.xml`:

	<meta-data android:name="billing.service" android:value="bazaar.BazaarIabService" />
	<activity android:name="com.bazaar.BazaarIABProxyActivity" android:theme="@android:style/Theme.Translucent.NoTitleBar.Fullscreen" />
	
Also add the required permissions to your manifest:

	<uses-permission android:name="android.permission.INTERNET" />
	<uses-permission android:name="com.farsitel.bazaar.permission.PAY_THROUGH_BAZAAR" />