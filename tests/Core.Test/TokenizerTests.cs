using BigRedProf.Data.Core;
using System;
using System.Linq;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class TokenizerTests
	{
		#region test helpers
		private class TestEntity
		{
			public Guid Id { get; set; }
			public string Name { get; set; }
		}
		#endregion

		#region methods
		[Fact]
		[Trait("Region", "methods")]
		public void DefineToken_ShouldThrowWhenTokenIsNull()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					tokenizer.DefineToken(null, "Huskers");
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void DefineToken_ShouldThrowWhenModelIsNull()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					tokenizer.DefineToken(new Code("0111"), null);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void DefineToken_ShouldThrowWhenTokenIsAlreadyDefined()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			tokenizer.DefineToken(new Code("0111"), "Nebraska Cornhuskers");

			Assert.Throws<ArgumentException>(
				() =>
				{
					tokenizer.DefineToken(new Code("0111"), "Ohio State Buckeyes");
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void DefineToken_ShouldThrowWhenModelIsAlreadyTokenized()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			tokenizer.DefineToken(new Code("0111"), "Nebraska Cornhuskers");

			Assert.Throws<ArgumentException>(
				() =>
				{
					tokenizer.DefineToken(new Code("1000"), "Nebraska Cornhuskers");
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void RedefineToken_ShouldReplaceTheModelBehindAToken()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			Code token = new Code("0111");
			tokenizer.DefineToken(token, "Nebraska Cornhuskers");

			tokenizer.RedefineToken(token, "Ohio State Buckeyes");

			Assert.Equal("Ohio State Buckeyes", tokenizer.GetModel(token));
			Assert.Equal(token, tokenizer.GetToken("Ohio State Buckeyes"));

			// the displaced model must no longer resolve to the token
			Assert.False(tokenizer.IsModelTokenized("Nebraska Cornhuskers"));
			Assert.Equal(1, tokenizer.Count);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void RedefineToken_ShouldMoveAModelToANewToken()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			Code oldToken = new Code("0111");
			Code newToken = new Code("1000");
			tokenizer.DefineToken(oldToken, "Nebraska Cornhuskers");

			tokenizer.RedefineToken(newToken, "Nebraska Cornhuskers");

			Assert.Equal(newToken, tokenizer.GetToken("Nebraska Cornhuskers"));
			Assert.Equal("Nebraska Cornhuskers", tokenizer.GetModel(newToken));

			// the old token must no longer resolve to the model
			Assert.False(tokenizer.IsTokenDefined(oldToken));
			Assert.Equal(1, tokenizer.Count);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void RedefineToken_ShouldBeIdempotentForAnExistingDefinition()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			Code token = new Code("0111");
			tokenizer.DefineToken(token, "Nebraska Cornhuskers");

			tokenizer.RedefineToken(token, "Nebraska Cornhuskers");

			Assert.Equal("Nebraska Cornhuskers", tokenizer.GetModel(token));
			Assert.Equal(token, tokenizer.GetToken("Nebraska Cornhuskers"));
			Assert.Equal(1, tokenizer.Count);
		}
		#endregion

		#region properties
		[Fact]
		[Trait("Region", "properties")]
		public void CountModelsAndTokens_ShouldExposeTheDefinedTokens()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			Assert.Equal(0, tokenizer.Count);
			Assert.Empty(tokenizer.Models);
			Assert.Empty(tokenizer.Tokens);

			Code huskersToken = new Code("0111");
			Code buckeyesToken = new Code("1001");
			tokenizer.DefineToken(huskersToken, "Nebraska Cornhuskers");
			tokenizer.DefineToken(buckeyesToken, "Ohio State Buckeyes");

			Assert.Equal(2, tokenizer.Count);
			Assert.Contains("Nebraska Cornhuskers", tokenizer.Models);
			Assert.Contains("Ohio State Buckeyes", tokenizer.Models);
			Assert.Contains(
				tokenizer.Tokens,
				kvp => kvp.Key == huskersToken && kvp.Value == "Nebraska Cornhuskers"
			);
			Assert.Contains(
				tokenizer.Tokens,
				kvp => kvp.Key == buckeyesToken && kvp.Value == "Ohio State Buckeyes"
			);
		}
		#endregion

		#region token allocation
		[Fact]
		[Trait("Region", "token allocation")]
		public void AllocateNextToken_ShouldAssignTheCanonicalSequence()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();

			// ordinal n gets the binary representation of n + 1
			Assert.Equal(new Code("1"), tokenizer.AllocateNextToken("Illinois Fighting Illini"));
			Assert.Equal(new Code("10"), tokenizer.AllocateNextToken("Indiana Hoosiers"));
			Assert.Equal(new Code("11"), tokenizer.AllocateNextToken("Iowa Hawkeyes"));
			Assert.Equal(new Code("100"), tokenizer.AllocateNextToken("Maryland Terrapins"));
			Assert.Equal(new Code("101"), tokenizer.AllocateNextToken("Michigan State Spartans"));

			Assert.Equal(5, tokenizer.Count);
		}

		[Fact]
		[Trait("Region", "token allocation")]
		public void AllocateNextToken_ShouldSkipPinnedTokens()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			tokenizer.DefineToken(new Code("10"), "Nebraska Cornhuskers");

			Assert.Equal(new Code("1"), tokenizer.AllocateNextToken("Illinois Fighting Illini"));
			Assert.Equal(new Code("11"), tokenizer.AllocateNextToken("Indiana Hoosiers"));
			Assert.Equal(new Code("100"), tokenizer.AllocateNextToken("Iowa Hawkeyes"));
		}

		[Fact]
		[Trait("Region", "token allocation")]
		public void AllocateNextToken_ShouldThrowWhenModelIsNull()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					tokenizer.AllocateNextToken(null);
				}
			);
		}

		[Fact]
		[Trait("Region", "token allocation")]
		public void AllocateNextToken_ShouldThrowWhenModelIsAlreadyTokenized()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			tokenizer.AllocateNextToken("Nebraska Cornhuskers");

			Assert.Throws<ArgumentException>(
				() =>
				{
					tokenizer.AllocateNextToken("Nebraska Cornhuskers");
				}
			);
		}
		#endregion

		#region freezing
		[Fact]
		[Trait("Region", "freezing")]
		public void Freeze_ShouldMakeDefinitionsThrowAndKeepReadsWorking()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			Code token = tokenizer.AllocateNextToken("Nebraska Cornhuskers");

			Assert.False(tokenizer.IsFrozen);
			tokenizer.Freeze();
			Assert.True(tokenizer.IsFrozen);

			Assert.Throws<InvalidOperationException>(
				() =>
				{
					tokenizer.DefineToken(new Code("1000"), "Ohio State Buckeyes");
				}
			);
			Assert.Throws<InvalidOperationException>(
				() =>
				{
					tokenizer.RedefineToken(token, "Ohio State Buckeyes");
				}
			);
			Assert.Throws<InvalidOperationException>(
				() =>
				{
					tokenizer.AllocateNextToken("Ohio State Buckeyes");
				}
			);

			// reads keep working after the freeze
			Assert.Equal("Nebraska Cornhuskers", tokenizer.GetModel(token));
			Assert.Equal(token, tokenizer.GetToken("Nebraska Cornhuskers"));
			Assert.True(tokenizer.IsTokenDefined(token));
			Assert.Equal(1, tokenizer.Count);
			Assert.Contains("Nebraska Cornhuskers", tokenizer.Models);
		}

		[Fact]
		[Trait("Region", "freezing")]
		public void Freeze_ShouldBeIdempotent()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			tokenizer.Freeze();
			tokenizer.Freeze();

			Assert.True(tokenizer.IsFrozen);
		}

		[Fact]
		[Trait("Region", "freezing")]
		public void ConcurrentReadsDuringHydration_ShouldNotThrow()
		{
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			int modelCount = 5000;

			System.Threading.Tasks.Task writer = System.Threading.Tasks.Task.Run(
				() =>
				{
					for (int i = 0; i < modelCount; ++i)
						tokenizer.AllocateNextToken($"model {i}");
				}
			);

			System.Threading.Tasks.Task reader = System.Threading.Tasks.Task.Run(
				() =>
				{
					Code firstToken = new Code("1");
					while (!writer.IsCompleted)
					{
						string model;
						if (tokenizer.TryGetModel(firstToken, out model))
							Assert.Equal("model 0", model);

						int count = tokenizer.Count;
						Assert.InRange(count, 0, modelCount);
					}
				}
			);

			System.Threading.Tasks.Task.WaitAll(writer, reader);
			Assert.Equal(modelCount, tokenizer.Count);
		}
		#endregion

		#region identity-keyed tokenizers
		[Fact]
		[Trait("Region", "identity-keyed tokenizers")]
		public void IdentitySelector_ShouldMatchModelsById()
		{
			Tokenizer<TestEntity> tokenizer = new Tokenizer<TestEntity>(e => e.Id);
			Guid id = Guid.NewGuid();
			Code token = new Code("0111");
			tokenizer.DefineToken(token, new TestEntity() { Id = id, Name = "original instance" });

			// a different object instance with the same identity, as when a model is
			// decoded off the wire, must resolve to the same token
			TestEntity decodedInstance = new TestEntity() { Id = id, Name = "decoded instance" };

			Assert.True(tokenizer.IsModelTokenized(decodedInstance));
			Assert.Equal(token, tokenizer.GetToken(decodedInstance));
		}

		[Fact]
		[Trait("Region", "identity-keyed tokenizers")]
		public void IdentitySelector_ShouldTreatSameIdAsSameModel()
		{
			Tokenizer<TestEntity> tokenizer = new Tokenizer<TestEntity>(e => e.Id);
			Guid id = Guid.NewGuid();
			tokenizer.DefineToken(new Code("0111"), new TestEntity() { Id = id, Name = "first" });

			// two entries sharing an identity are the same model, so this is a collision,
			// not a second definition
			Assert.Throws<ArgumentException>(
				() =>
				{
					tokenizer.DefineToken(new Code("1000"), new TestEntity() { Id = id, Name = "second" });
				}
			);
		}

		[Fact]
		[Trait("Region", "identity-keyed tokenizers")]
		public void IdentitySelectorConstructor_ShouldThrowWhenSelectorIsNull()
		{
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					new Tokenizer<TestEntity>((Func<TestEntity, object>)null);
				}
			);
		}

		[Fact]
		[Trait("Region", "identity-keyed tokenizers")]
		public void ComparerConstructor_ShouldThrowWhenComparerIsNull()
		{
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					new Tokenizer<string>((System.Collections.Generic.IEqualityComparer<string>)null);
				}
			);
		}
		#endregion
	}
}
