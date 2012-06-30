del ..\js\liveresults.min.js
type ..\js\*.js | jsmin > ..\js\liveresults.min.tmp
ren ..\js\liveresults.min.tmp liveresults.min.js