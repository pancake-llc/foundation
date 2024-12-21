#if UNITY_EDITOR
using Sisus.Init.Internal;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
	public interface IInitializerEditorOnly : IInitializer
	{
		bool IsAsync { get; }
		bool WasJustReset { get; set; }
		bool ShowNullArgumentGuard { get; }
		/// <summary>
		/// Does this initializer have null argument guard support?
		/// <para>
		/// This should return <see langword="true"/> if it is possible for the user to enable
		/// null argument guard on this initializer, even if the user has opted to turn it off.
		/// </para>
		/// </summary>
		bool CanInitTargetWhenInactive { get; }

		bool CanGuardAgainstNull { get; }
		NullArgumentGuard NullArgumentGuard { get; set; }
		string NullGuardFailedMessage { get; set; }
		NullGuardResult EvaluateNullGuard();
		bool MultipleInitializersPerTargetAllowed { get; }
		void SetReleaseArgumentOnDestroy(Arguments argument, bool shouldRelease);
		void SetIsArgumentAsync(Arguments argument, bool isAsync);
	}

	internal interface IInitializerEditorOnly<TClient> : IInitializerEditorOnly { }

	internal interface IInitializerEditorOnly<TClient, TArgument> : IInitializerEditorOnly<TClient>
	{
		TArgument Argument { get; set; }
		void OnReset(ref TArgument argument);
	}

	internal interface IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument> : IInitializerEditorOnly<TClient>
	{
		TFirstArgument FirstArgument { get; set; }
		TSecondArgument SecondArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument);
	}

	internal interface IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument> : IInitializerEditorOnly<TClient>
	{
		TFirstArgument FirstArgument { get; set; }
		TSecondArgument SecondArgument { get; set; }
		TThirdArgument ThirdArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument);
	}

	internal interface IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> : IInitializerEditorOnly<TClient>
	{
		TFirstArgument FirstArgument { get; set; }
		TSecondArgument SecondArgument { get; set; }
		TThirdArgument ThirdArgument { get; set; }
		TFourthArgument FourthArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument);
	}

	internal interface IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> : IInitializerEditorOnly<TClient>
	{
		TFirstArgument FirstArgument { get; set; }
		TSecondArgument SecondArgument { get; set; }
		TThirdArgument ThirdArgument { get; set; }
		TFourthArgument FourthArgument { get; set; }
		TFifthArgument FifthArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument);
	}

	internal interface IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> : IInitializerEditorOnly<TClient>
	{
		TFirstArgument FirstArgument { get; set; }
		TSecondArgument SecondArgument { get; set; }
		TThirdArgument ThirdArgument { get; set; }
		TFourthArgument FourthArgument { get; set; }
		TFifthArgument FifthArgument { get; set; }
		TSixthArgument SixthArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument);
	}

	internal interface IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument> : IInitializerEditorOnly<TClient>
	{
		TFirstArgument FirstArgument { get; set; }
		TSecondArgument SecondArgument { get; set; }
		TThirdArgument ThirdArgument { get; set; }
		TFourthArgument FourthArgument { get; set; }
		TFifthArgument FifthArgument { get; set; }
		TSixthArgument SixthArgument { get; set; }
		TSeventhArgument SeventhArgument { get; set; }

		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument, ref TSeventhArgument seventhArgument);
	}

	internal interface IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument> : IInitializerEditorOnly<TClient>
	{
		TFirstArgument FirstArgument { get; set; }
		TSecondArgument SecondArgument { get; set; }
		TThirdArgument ThirdArgument { get; set; }
		TFourthArgument FourthArgument { get; set; }
		TFifthArgument FifthArgument { get; set; }
		TSixthArgument SixthArgument { get; set; }
		TSeventhArgument SeventhArgument { get; set; }
		TEighthArgument EighthArgument { get; set; }

		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument, ref TSeventhArgument seventhArgument, ref TEighthArgument eighthArgument);
	}

	internal interface IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument> : IInitializerEditorOnly<TClient>
	{
		TFirstArgument FirstArgument { get; set; }
		TSecondArgument SecondArgument { get; set; }
		TThirdArgument ThirdArgument { get; set; }
		TFourthArgument FourthArgument { get; set; }
		TFifthArgument FifthArgument { get; set; }
		TSixthArgument SixthArgument { get; set; }
		TSeventhArgument SeventhArgument { get; set; }
		TEighthArgument EighthArgument { get; set; }
		TNinthArgument NinthArgument { get; set; }

		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument, ref TSeventhArgument seventhArgument, ref TEighthArgument eighthArgument, ref TNinthArgument ninthArgument);
	}

	internal interface IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument> : IInitializerEditorOnly<TClient>
	{
		TFirstArgument FirstArgument { get; set; }
		TSecondArgument SecondArgument { get; set; }
		TThirdArgument ThirdArgument { get; set; }
		TFourthArgument FourthArgument { get; set; }
		TFifthArgument FifthArgument { get; set; }
		TSixthArgument SixthArgument { get; set; }
		TSeventhArgument SeventhArgument { get; set; }
		TEighthArgument EighthArgument { get; set; }
		TNinthArgument NinthArgument { get; set; }
		TTenthArgument TenthArgument { get; set; }

		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument, ref TSeventhArgument seventhArgument, ref TEighthArgument eighthArgument, ref TNinthArgument ninthArgument, ref TTenthArgument tenthArgument);
	}

	internal interface IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument> : IInitializerEditorOnly<TClient>
	{
		TFirstArgument FirstArgument { get; set; }
		TSecondArgument SecondArgument { get; set; }
		TThirdArgument ThirdArgument { get; set; }
		TFourthArgument FourthArgument { get; set; }
		TFifthArgument FifthArgument { get; set; }
		TSixthArgument SixthArgument { get; set; }
		TSeventhArgument SeventhArgument { get; set; }
		TEighthArgument EighthArgument { get; set; }
		TNinthArgument NinthArgument { get; set; }
		TTenthArgument TenthArgument { get; set; }
		TEleventhArgument EleventhArgument { get; set; }

		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument, ref TSeventhArgument seventhArgument, ref TEighthArgument eighthArgument, ref TNinthArgument ninthArgument, ref TTenthArgument tenthArgument, ref TEleventhArgument eleventhArgument);
	}

	internal interface IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument> : IInitializerEditorOnly<TClient>
	{
		TFirstArgument FirstArgument { get; set; }
		TSecondArgument SecondArgument { get; set; }
		TThirdArgument ThirdArgument { get; set; }
		TFourthArgument FourthArgument { get; set; }
		TFifthArgument FifthArgument { get; set; }
		TSixthArgument SixthArgument { get; set; }
		TSeventhArgument SeventhArgument { get; set; }
		TEighthArgument EighthArgument { get; set; }
		TNinthArgument NinthArgument { get; set; }
		TTenthArgument TenthArgument { get; set; }
		TEleventhArgument EleventhArgument { get; set; }
		TTwelfthArgument TwelfthArgument { get; set; }

		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument, ref TSeventhArgument seventhArgument, ref TEighthArgument eighthArgument, ref TNinthArgument ninthArgument, ref TTenthArgument tenthArgument, ref TEleventhArgument eleventhArgument, ref TTwelfthArgument twelfthArgument);
	}

	internal interface IAnyInitializerEditorOnly<TClient, TArgument> : IInitializerEditorOnly<TClient> where TClient : Object
	{
		Any<TArgument> Argument { get; set; }
		void OnReset(ref TArgument argument);
	}

	internal interface IAnyInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument> : IInitializerEditorOnly<TClient> where TClient : Object
	{
		Any<TFirstArgument> FirstArgument { get; set; }
		Any<TSecondArgument> SecondArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument);
	}

	internal interface IAnyInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument> : IInitializerEditorOnly<TClient> where TClient : Object
	{
		Any<TFirstArgument> FirstArgument { get; set; }
		Any<TSecondArgument> SecondArgument { get; set; }
		Any<TThirdArgument> ThirdArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument);
	}

	internal interface IAnyInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> : IInitializerEditorOnly<TClient> where TClient : Object
	{
		Any<TFirstArgument> FirstArgument { get; set; }
		Any<TSecondArgument> SecondArgument { get; set; }
		Any<TThirdArgument> ThirdArgument { get; set; }
		Any<TFourthArgument> FourthArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument);
	}

	internal interface IAnyInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> : IInitializerEditorOnly<TClient> where TClient : Object
	{
		Any<TFirstArgument> FirstArgument { get; set; }
		Any<TSecondArgument> SecondArgument { get; set; }
		Any<TThirdArgument> ThirdArgument { get; set; }
		Any<TFourthArgument> FourthArgument { get; set; }
		Any<TFifthArgument> FifthArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument);
	}

	internal interface IAnyInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> : IInitializerEditorOnly<TClient> where TClient : Object
	{
		Any<TFirstArgument> FirstArgument { get; set; }
		Any<TSecondArgument> SecondArgument { get; set; }
		Any<TThirdArgument> ThirdArgument { get; set; }
		Any<TFourthArgument> FourthArgument { get; set; }
		Any<TFifthArgument> FifthArgument { get; set; }
		Any<TSixthArgument> SixthArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument);
	}

	internal interface IAnyInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument> : IInitializerEditorOnly<TClient> where TClient : Object
	{
		Any<TFirstArgument> FirstArgument { get; set; }
		Any<TSecondArgument> SecondArgument { get; set; }
		Any<TThirdArgument> ThirdArgument { get; set; }
		Any<TFourthArgument> FourthArgument { get; set; }
		Any<TFifthArgument> FifthArgument { get; set; }
		Any<TSixthArgument> SixthArgument { get; set; }
		Any<TSixthArgument> SeventhArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument, ref TSeventhArgument seventhArgument);
	}

	internal interface IAnyInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument> : IInitializerEditorOnly<TClient> where TClient : Object
	{
		Any<TFirstArgument> FirstArgument { get; set; }
		Any<TSecondArgument> SecondArgument { get; set; }
		Any<TThirdArgument> ThirdArgument { get; set; }
		Any<TFourthArgument> FourthArgument { get; set; }
		Any<TFifthArgument> FifthArgument { get; set; }
		Any<TSixthArgument> SixthArgument { get; set; }
		Any<TSeventhArgument> SeventhArgument { get; set; }
		Any<TEighthArgument> EighthArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument, ref TSeventhArgument seventhArgument, ref TEighthArgument eighthArgument);
	}

	internal interface IAnyInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument> : IInitializerEditorOnly<TClient> where TClient : Object
	{
		Any<TFirstArgument> FirstArgument { get; set; }
		Any<TSecondArgument> SecondArgument { get; set; }
		Any<TThirdArgument> ThirdArgument { get; set; }
		Any<TFourthArgument> FourthArgument { get; set; }
		Any<TFifthArgument> FifthArgument { get; set; }
		Any<TSixthArgument> SixthArgument { get; set; }
		Any<TSeventhArgument> SeventhArgument { get; set; }
		Any<TEighthArgument> EighthArgument { get; set; }
		Any<TSixthArgument> NinthArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument, ref TSeventhArgument seventhArgument, ref TEighthArgument eighthArgument, ref TNinthArgument ninthArgument);
	}

	internal interface IAnyInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument> : IInitializerEditorOnly<TClient> where TClient : Object
	{
		Any<TFirstArgument> FirstArgument { get; set; }
		Any<TSecondArgument> SecondArgument { get; set; }
		Any<TThirdArgument> ThirdArgument { get; set; }
		Any<TFourthArgument> FourthArgument { get; set; }
		Any<TFifthArgument> FifthArgument { get; set; }
		Any<TSixthArgument> SixthArgument { get; set; }
		Any<TSeventhArgument> SeventhArgument { get; set; }
		Any<TEighthArgument> EighthArgument { get; set; }
		Any<TSixthArgument> NinthArgument { get; set; }
		Any<TSixthArgument> TenthArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument, ref TSeventhArgument seventhArgument, ref TEighthArgument eighthArgument, ref TNinthArgument ninthArgument, ref TTenthArgument tenthArgument);
	}

	internal interface IAnyInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument> : IInitializerEditorOnly<TClient> where TClient : Object
	{
		Any<TFirstArgument> FirstArgument { get; set; }
		Any<TSecondArgument> SecondArgument { get; set; }
		Any<TThirdArgument> ThirdArgument { get; set; }
		Any<TFourthArgument> FourthArgument { get; set; }
		Any<TFifthArgument> FifthArgument { get; set; }
		Any<TSixthArgument> SixthArgument { get; set; }
		Any<TSeventhArgument> SeventhArgument { get; set; }
		Any<TEighthArgument> EighthArgument { get; set; }
		Any<TSixthArgument> NinthArgument { get; set; }
		Any<TSixthArgument> TenthArgument { get; set; }
		Any<TSixthArgument> EleventhArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument, ref TSeventhArgument seventhArgument, ref TEighthArgument eighthArgument, ref TNinthArgument ninthArgument, ref TTenthArgument tenthArgument, ref TEleventhArgument eleventhArgument);
	}

	internal interface IAnyInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument> : IInitializerEditorOnly<TClient> where TClient : Object
	{
		Any<TFirstArgument> FirstArgument { get; set; }
		Any<TSecondArgument> SecondArgument { get; set; }
		Any<TThirdArgument> ThirdArgument { get; set; }
		Any<TFourthArgument> FourthArgument { get; set; }
		Any<TFifthArgument> FifthArgument { get; set; }
		Any<TSixthArgument> SixthArgument { get; set; }
		Any<TSeventhArgument> SeventhArgument { get; set; }
		Any<TEighthArgument> EighthArgument { get; set; }
		Any<TSixthArgument> NinthArgument { get; set; }
		Any<TSixthArgument> TenthArgument { get; set; }
		Any<TSixthArgument> EleventhArgument { get; set; }
		Any<TSixthArgument> TwelfthArgument { get; set; }
		void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument, ref TSeventhArgument seventhArgument, ref TEighthArgument eighthArgument, ref TNinthArgument ninthArgument, ref TTenthArgument tenthArgument, ref TEleventhArgument eleventhArgument, ref TTwelfthArgument twelfthArgument);
	}
}
#endif