
function engine(kart, sounds)
	-- sounds is an array, ordered by the Sound{} blocks in the .thing file
	-- you can either loop over all of them, or do different things to each one
	
	-- here's an example of a loop
	for i = 0, sounds.Length - 1 do
		sounds[i].PlaybackSpeed = math.min((math.abs(kart.VehicleSpeed) * 0.0075) + 0.7, 3)
	end
	
	-- and if you just wanted to do something to one sound, it would look like this:
	-- sounds[0].PlaybackSpeed = blahblah
end

-- this is called when we spawn a kart
-- with lua, you can only use variables/functions you've defined above this
-- so you probably want this at the bottom of the file
function TwiCutlass(lthing)
	addSoundFrameFunction(lthing, engine)
end