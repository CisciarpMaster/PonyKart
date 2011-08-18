-- first we tell lua about our c# program
luanet.load_assembly("Lymph")

print("test.lua has been run")
-- then we do stuff!

function triggerTestFunction(s1, s2, tf)
	if isEnterFlag(tf) then
		print("enter")
	else
		print("leave")
	end
	print(getBodyName(s2))
end

hookScriptToTriggerRegion("test trigger area", "media/scripts/testevent.lua")
hookFunctionToTriggerRegion("test trigger area", triggerTestFunction)



function testfunction(e)
	print("=== function event hook test successful!")
end