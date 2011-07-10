-- first we tell lua about our c# program
luanet.load_assembly("Lymph")

print("Running test trigger regions.lua")

-- this will not get run just by running this file - it will only run if we call it directly, which is what the event is for :D
-- it DOES matter where this function is - it #MUST# be above where you hook it to an event, like we're doing here!
function testfunction(triggerShape, otherShape, triggerFlags)
	if Triggers.isEnterFlag(triggerFlags) then
		print("enter")
	else
		print("leave")
	end
	print(otherShape.Actor.Name)
end

-- hook our function to the event
Triggers.hookFunctionToTriggerArea("test trigger area", testfunction)

