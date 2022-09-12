## Local Notification


UPM Package
---
### Install via git URL

Requires a version of unity that supports path query parameter for git packages (Unity >= 2019.3.4f1, Unity >= 2020.1a21).
You can add 

```cs
https://github.com/snorluxe/local-notification.git?path=Assets/_Root
```
 to PackageManager

![image](https://user-images.githubusercontent.com/44673303/112711380-791f6180-8efa-11eb-953f-d92d2bc93f0f.png)

![image](https://user-images.githubusercontent.com/44673303/112711396-99e7b700-8efa-11eb-9548-a6ab1487d887.png)

or add 
```cs
"com.snorlax.local-notification": "https://github.com/snorluxe/local-notification.git?path=Assets/_Root"
```
to `Packages/manifest.json`.

If you want to set a target version. lance uses the `year.month.day` release tag so you can specify a version like `#2022.2.17`. For example 

```cs
https://github.com/snorluxe/local-notification.git?path=Assets/_Root#2022.5.13
```


### Install via manifest.json

Add the following dependencies in manifest

```cs
"com.snorlax.local-notification": "https://github.com/snorluxe/local-notification.git?path=Assets/_Root#2022.5.13",
```


### Usages

- Add component `NotificationConsole` into object has dont destroy to reschedule each time go to background and cancel when back to forceground

- Type `Repeat` will fire notification after each number `Minute`

![image](https://user-images.githubusercontent.com/44673303/141402003-88e7e3f7-bde2-4513-a7bf-d4fc4539ca02.png)


- Event `OnUpdateDeliveryTime` to use when you want send notification one time with diffirent custom `Minute` each time such as fire notification in game idle when building house completed. In case you need write your custom method
to assign to event by call to API `public void UpdateDeliveryTimeBy(string id, int customTimeSchedule = -1)`