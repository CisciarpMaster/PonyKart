-- first we tell lua about our c# program
luanet.load_assembly("Lymph")

print("test.lua has been run")
-- then we do stuff!

function testfunctionn(s1, s2, tf)
	if isTriggerEnterFlag(tf) then
		print("enter")
	else
		print("leave")
	end
	print(s2.Actor.Name)
end

hookScriptToTriggerArea("test trigger area", "media/scripts/testevent.lua")
hookFunctionToTriggerArea("test trigger area", testfunctionn)



function testfunction(e)
	print("=== function event hook test successful!")
end