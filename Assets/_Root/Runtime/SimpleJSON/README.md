# SimpleJson

## How to install

Add the lines below to `Packages/manifest.json`


- for version `1.0.3` with unity 2021 or newer
   
```csharp
"com.pancake.simplejson": "https://github.com/pancake-llc/SimpleJSON.git?path=Assets/_Root#1.0.3",
```


## Usages

- Serialize
```c#
public void Serialize(T data, Stream writer)
{
	var jsonNode = JSON.Parse(JsonUtility.ToJson(data));
	jsonNode.SaveToBinaryStream(writer);
}
```

- Deserialize
```c#
public T Deserialize(Stream reader)
{
	var jsonData = JSONNode.LoadFromBinaryStream(reader);
	string json = jsonData.ToString()
    // ... 
}
```


```c#
var jsonNode = JSON.Parse(jsonString);
if (jsonNode == null) return jsonString;

object jsonObject = jsonNode;
var version = jsonNode["version"].AsInt;
```