/* TEMPLATE GENERATED TESTCASE FILE
Filename: CWE190_Integer_Overflow__Short_max_multiply_61a.cs
Label Definition File: CWE190_Integer_Overflow.label.xml
Template File: sources-sinks-61a.tmpl.cs
*/
/*
 * @description
 * CWE: 190 Integer Overflow
 * BadSource: max Set data to the max value for short
 * GoodSource: A hardcoded non-zero, non-min, non-max, even number
 * Sinks: multiply
 *    GoodSink: Ensure there will not be an overflow before multiplying data by 2
 *    BadSink : If data is positive, multiply by 2, which can cause an overflow
 * Flow Variant: 61 Data flow: data returned from one method to another in different classes in the same package
 *
 * */

using TestCaseSupport;
using System;

using System.Web;

namespace testcases.CWE190_Integer_Overflow
{
class CWE190_Integer_Overflow__Short_max_multiply_61a : AbstractTestCase
{
#if (!OMITBAD)
    public override void Bad()
    {
        short data = CWE190_Integer_Overflow__Short_max_multiply_61b.BadSource();
        if(data > 0) /* ensure we won't have an underflow */
        {
            /* POTENTIAL FLAW: if (data*2) > short.MaxValue, this will overflow */
            short result = (short)(data * 2);
            IO.WriteLine("result: " + result);
        }
    }
#endif //omitbad
#if (!OMITGOOD)
    public override void Good()
    {
        GoodG2B();
        GoodB2G();
    }

    /* goodG2B() - use goodsource and badsink */
    private static void GoodG2B()
    {
        short data = CWE190_Integer_Overflow__Short_max_multiply_61b.GoodG2BSource();
        if(data > 0) /* ensure we won't have an underflow */
        {
            /* POTENTIAL FLAW: if (data*2) > short.MaxValue, this will overflow */
            short result = (short)(data * 2);
            IO.WriteLine("result: " + result);
        }
    }

    /* goodB2G() - use badsource and goodsink */
    private static void GoodB2G()
    {
        short data = CWE190_Integer_Overflow__Short_max_multiply_61b.GoodB2GSource();
        if(data > 0) /* ensure we won't have an underflow */
        {
            /* FIX: Add a check to prevent an overflow from occurring */
            if (data < (short.MaxValue/2))
            {
                short result = (short)(data * 2);
                IO.WriteLine("result: " + result);
            }
            else
            {
                IO.WriteLine("data value is too large to perform multiplication.");
            }
        }
    }
#endif //omitgood
}
}
