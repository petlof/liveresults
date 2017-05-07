EmmaClient is a client-server system for publishing liveresults from Orienteering events. It have been used on many major events such as JWOC, WOC, EOC,..
**Current Build status**

**International events references:** WOC2016, WOC2015, WOC2014, EOC2012, WUOC2010, JWOC2008, JWOC2007, WOC2006, WOC2004
**National events references:** FIN-5 2012, Swedish Elitserien 2006-, and 500+ other events since 2006

**2016-09-25 - EmmaClient / LiveResults used for the third WOC in a row**
 With WOC2016 in Sweden/Strömstad it was the third WOC in a row that used EmmaClient as the official system for LiveResults. This time timing was done by EQTiming and EMIT with SSFTiming as official results system and the OLA system used for Punch evaluation.
Support for SSFTiming was added to EmmaClient with an adapter that have the same small delay (< 1 second) as the OLA-adapter giving the internet audience live results with very little delay.
For this WOC some small additions was also made to the web-display were now diff-times on each split control is showed. 

The hosting of the LiveResults-service for WOC 2016 http://liveresults.woc2016.se was offered and delivered with success with an scalable cloud-environment. Also the traffic for the main page www.woc2016.se was cached by the liveresults server to minimize the load of the CMS-system delivering the official web page

If you are interested in liveresults and operations for your event, contact: peter@lofas.se to get more information

**2015-08-07 - Success during WOC 2015 in Scotland**
As well as 2014, EmmaClient was the base for the liveresults-service during WOC 2015. Adaptions for the IOF-XML exports from the SportIdent.UK software AutoDownload was implemented and is now part of the EmmaClient baseline. Also some nice features were added to the live-results GUI where display of running times for all runners in real time and better sorting during mass-start-races and relays were added.

The hosting of the LiveResults-service for WOC 2015 http://liveresults.woc2015.org were also offered and delivered with success with an scalable cloud-environment.

If you are interested in liveresults and operations for your event, contact: peter@lofas.se to get more information

**2014-07-13 - Successful usage during WOC 2014 in Italy**
I got an late request from WOC2014 about using the EmmaClient solution as the official live result system for WOC in Italy. It was accepted and consisted of both integrations to the event software from RaCom and hosting and operations during the WOC week.

Everything worked perfect and is proves how easy and scalable the EmmaClient solution is for everything from small local events to events with huge interest online such as WOC and EOC. 

Operations of the system during the WOC-week was done (similar to earlier operations) in the Amazon Cloud platform (EC2) with Ubuntu 14.04 servers. The plattform performed very well and maxed during the long distance at 254Mbit / s traffic out from the load balancer with response times kept as low as 20ms

Overall, it was a very successful week with great competitions and great experience online!

If you are interested in liveresults and operations for your event, contact: peter@lofas.se to get more information

**2012-07-01 Clubresults and FIN-5**
A clubresults view have been added to the liveresults where you can track all results for a given club. Also some basic support for MultiDay events with total-results have been added to the system

This year (2012) FIN-5 (http://www.fin5.fi) used the liveresults-system for all their liveresults during the competition

**2012-06-07 Obasen.nu upgraded**
Obasen.nu upgraded to latest version and API-documentation have been published

**2012-06-01 EOC 2012 Success**
Liveresults were successfully used during the EOC-2012 event. Lots and lots of viewers without any problem. During the long final there were about 1000 pageviews/second on the liveresults.

During EOC we used liveresults deployed on Could-Servers to have unlimited scalabilty. If you are interested in knowing more about this, contact Peter Löfås (peter@lofas.se)

**2012-05-09 Documentation is updated**
Documentation have been updated with information about the process of publishing liveresults from OLA and OE2010
