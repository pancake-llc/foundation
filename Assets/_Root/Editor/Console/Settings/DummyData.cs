namespace Needle.Console
{
	internal static class DummyData
	{
		public const string SyntaxHighlightVisualization = @"InvalidOperationException: Collection was modified; enumeration operation may not execute.
System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
bool System.Collections.Generic.List<int>+Enumerator.MoveNextRare()
bool System.Collections.Generic.List<int>+Enumerator.MoveNext()
IEnumerable<string> Demystify._Tests.Program.Iterator(int startAt)+MoveNext() in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 46
bool System.Linq.Enumerable+SelectEnumerableIterator<string, string>.MoveNext()
string string.Join(string separator, IEnumerable<string> values)
string Demystify._Tests.Program+GenericClass<byte>.GenericMethod<int>(ref int value) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 144
Rethrow as Exception: Collection was modified; enumeration operation may not execute.
System.Exception: Collection was modified; enumeration operation may not execute.
string Demystify._Tests.Program+GenericClass<byte>.GenericMethod<int>(ref int value) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 148
async Task<string> Demystify._Tests.Program.MethodAsync(int value) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 59
string System.Runtime.CompilerServices.TaskAwaiter<string>.GetResult()
async void Demystify._Tests.Program+<<MethodAsync>g__MethodLocalAsync|9_0>d<string>.MoveNext() in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 69
string System.Runtime.CompilerServices.TaskAwaiter<string>.GetResult()
async void Demystify._Tests.Program+<MethodAsync>d__9<string>.MoveNext() in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 65
string System.Runtime.CompilerServices.TaskAwaiter<string>.GetResult()
(string val, bool) Demystify._Tests.Program.Method(string value)+() => { } [0] in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 83
(string val, bool) Demystify._Tests.Program.Method(string value)+() => { } [1] in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 86
string Demystify._Tests.Program.RunLambda(Func<string> lambda) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 77
(string val, bool) Demystify._Tests.Program.Method(string value) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 86
string Demystify._Tests.Program.RefMethod(in string value)+LocalFuncRefReturn(ref <>c__DisplayClass14_0) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 105
string Demystify._Tests.Program.RefMethod(in string value)+LocalFuncParam(string s, ref <>c__DisplayClass14_0) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 100
string Demystify._Tests.Program.RefMethod(in string value) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 96
(string val, bool) Demystify._Tests.Program.s_func(string s, bool b) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 27
void Demystify._Tests.Program.s_action(string s, bool b) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 26
void Demystify._Tests.Program.Start((string val, bool) param) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 131
string Demystify._Tests.Program.Start()+LocalFunc1(long l) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 117
string Demystify._Tests.Program.Start()+LocalFunc2(bool b1, bool b2) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 122
string Demystify._Tests.Program.Start() in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 113
Demystify._Tests.Program()+() => { } in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 34
Demystify._Tests.Program(Action action)+(object s) => { } [1] in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 40
Demystify._Tests.Program(Action action)+(Action<object> lambda, object state) => { } in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 29
Demystify._Tests.Program(Action action)+(object state) => { } [0] in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 40
void Demystify._Tests.Program.RunAction(Action<object> lambda, object state) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 74
new Demystify._Tests.Program(Action action) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 40
new Demystify._Tests.Program() in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 34
   at void Demystify._Tests.Program.Main() in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 18 ---> System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
bool System.Collections.Generic.List<int>+Enumerator.MoveNextRare()
bool System.Collections.Generic.List<int>+Enumerator.MoveNext()
IEnumerable<string> Demystify._Tests.Program.Iterator(int startAt)+MoveNext() in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 46
bool System.Linq.Enumerable+SelectEnumerableIterator<string, string>.MoveNext()
string string.Join(string separator, IEnumerable<string> values)
string Demystify._Tests.Program+GenericClass<byte>.GenericMethod<int>(ref int value) in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 144
--- End of inner exception stack trace ---
void UnityEngine.DebugLogHandler.Internal_LogException(Exception ex, Object obj)
void UnityEngine.DebugLogHandler.LogException(Exception exception, Object context)
void UnityEngine.Logger.LogException(Exception exception, Object context)
void UnityEngine.Debug.LogException(Exception exception)
void Demystify._Tests.Program.Main() in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/Sample.cs:line 22
void ExceptionThrower.RunSample() in C:/git/demystify/modules/unity-demystify/projects/needle.demystify.extras/DemystifyTestCode/ExceptionThrower.cs:line 100
";
	}
}