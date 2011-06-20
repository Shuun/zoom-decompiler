// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;

public class MultidimensionalInitializerTests
{ 
	public void MultidimensionalInitExplicit()
	{
		int[,] expr_09 = new int[, ]
		{

	        {
	            1234,
	            3456,
	            12345,
	            34567
	        },

	        {
	            222223,
	            333332,
	            443344,
	            334433
	        },

	        {
	            201010,
	            10102020,
	            11111,
	            3334444
	        },

	        {
	            101212,
	            1231123,
	            234222,
	            6655
	        },

	        {
	            12311110,
	            123110,
	            1,
	            233330
	        },

	        {
	            0,
	            0,
	            1,
	            0
	        },

	        {
	            0,
	            1,
	            1,
	            1
	        },

	        {
	            0,
	            0,
	            0,
	            0
	        },

	        {
	            123120,
	            24343430,
	            555560,
	            77766660
	        },

	        {
	            12222,
	            123,
	            133333,
	            44221
	        },

	        {
	            23423420,
	            4444440,
	            555333,
	            0
	        },

	        {
	            444440,
	            10555555,
	            222220,
	            33330
	        },

	        {
	            1000020,
	            2222220,
	            444466661,
	            778760
	        },

	        {
	            1230,
	            223330,
	            199991,
	            88880
	        },

	        {
	            205,
	            405,
	            515,
	            5550
	        },

	        {
	            0,
	            0,
	            1,
	            0
			}
	    };
	}

	public void ArrayOfArrayOfArrayInit()
	{
		int[][,,] array = new int[][,,]
		{
			new int[, , ]
			{
				{
					{
						1,
						2,
						3
					},
					{ 
						4,
						5,
						6
					},
					{
						7,
						8,
						9
					}
				},
				{
					{
						11,
						12,
						13
					},
					{
						14,
						15,
						16
					},
					{
						17,
						18,
						19
					}
				}
			},

			new int[, , ]
			{
				{
					{
						21,
						22,
						23
					},
					{
						24,
						25,
						26
					},
					{
						27,
						28,
						29
					}
				},
				{
					{
						31,
						32,
						33
					},
					{
						34,
						35,
						36
					},
					{
						37,
						38,
						39
					}
				}
			}
	};
	}
}
