This is a .net port of ItDangerious https://github.com/mitsuhiko/itsdangerous/ Which is a port of the signing code from Django. 

Signed tokens are intended to be URL friendly and as short as possible. 

I've endevored to make comparasons constant time to avoid side channel attacks - but this is extremely difficlt to both do and measure. If you keep your tokens short this is unlikely to be an issue - but I'm no crypto guy. 

The Tests explain things - but you can either sign things with or without a timestamp. 

To generate a signed token you do:
	var timedsigner = new TimeStampSigner("testing");
	var signed = timedsigner.Sign("hello world");

To validate a token you've been given:
	var unsigned = timedsigner.Unsign(signed);

To check if a token has expired:
	signer.HasExpired(signed, 1).ShouldBe(true);

Signed tokens look like with a time stamp: 
	hello world:WfwWAw:y7sw1JWfkj9PsdjIvLA8TxYuXao

Or without a timestamp:
	hello world:y7sw1JWfkj9PsdjIvLA8TxYuXao
