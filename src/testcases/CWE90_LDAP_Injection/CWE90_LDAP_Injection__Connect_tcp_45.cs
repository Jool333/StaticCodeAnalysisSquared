/* TEMPLATE GENERATED TESTCASE FILE
Filename: CWE90_LDAP_Injection__Connect_tcp_45.cs
Label Definition File: CWE90_LDAP_Injection.label.xml
Template File: sources-sink-45.tmpl.cs
*/
/*
 * @description
 * CWE: 90 LDAP Injection
 * BadSource: Connect_tcp Read data using an outbound tcp connection
 * GoodSource: A hardcoded string
 * Sinks:
 *    BadSink : data concatenated into LDAP search, which could result in LDAP Injection
 * Flow Variant: 45 Data flow: data passed as a private class member variable from one function to another in the same class
 *
 * */

using TestCaseSupport;
using System;

using System.DirectoryServices;

using System.Web;

using System.IO;
using System.Net.Sockets;

namespace testcases.CWE90_LDAP_Injection
{

class CWE90_LDAP_Injection__Connect_tcp_45 : AbstractTestCase
{

    private string dataBad;
    private string dataGoodG2B;
#if (!OMITBAD)
    private void BadSink()
    {
        string data = dataBad;
        using (DirectoryEntry de = new DirectoryEntry())
        {
            /* POTENTIAL FLAW: data concatenated into LDAP search, which could result in LDAP Injection */
            using (DirectorySearcher search = new DirectorySearcher(de))
            {
                search.Filter = "(&(objectClass=user)(employeename=" + data + "))";
                search.PropertiesToLoad.Add("mail");
                search.PropertiesToLoad.Add("telephonenumber");
                SearchResult sresult = search.FindOne();
            }
        }
    }

    /* uses badsource and badsink */
    public override void Bad()
    {
        string data;
        data = ""; /* Initialize data */
        /* Read data using an outbound tcp connection */
        {
            try
            {
                /* Read data using an outbound tcp connection */
                using (TcpClient tcpConn = new TcpClient("host.example.org", 39544))
                {
                    /* read input from socket */
                    using (StreamReader sr = new StreamReader(tcpConn.GetStream()))
                    {
                        /* POTENTIAL FLAW: Read data using an outbound tcp connection */
                        data = sr.ReadLine();
                    }
                }
            }
            catch (IOException exceptIO)
            {
                IO.Logger.Log(NLog.LogLevel.Warn, exceptIO, "Error with stream reading");
            }
        }
        dataBad = data;
        BadSink();
    }
#endif //omitbad
#if (!OMITGOOD)
    public override void Good()
    {
        GoodG2B();
    }

    private void GoodG2BSink()
    {
        string data = dataGoodG2B;
        using (DirectoryEntry de = new DirectoryEntry())
        {
            /* POTENTIAL FLAW: data concatenated into LDAP search, which could result in LDAP Injection */
            using (DirectorySearcher search = new DirectorySearcher(de))
            {
                search.Filter = "(&(objectClass=user)(employeename=" + data + "))";
                search.PropertiesToLoad.Add("mail");
                search.PropertiesToLoad.Add("telephonenumber");
                SearchResult sresult = search.FindOne();
            }
        }
    }

    /* goodG2B() - use goodsource and badsink */
    private void GoodG2B()
    {
        string data;
        /* FIX: Use a hardcoded string */
        data = "foo";
        dataGoodG2B = data;
        GoodG2BSink();
    }
#endif //omitgood
}
}
