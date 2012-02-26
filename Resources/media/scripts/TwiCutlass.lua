
function engine(kart, sound)
	sound.PlaybackSpeed = math.min((math.abs(kart.VehicleSpeed) * 0.0075) + 0.7, 3)
end

-- this is called when we spawn a kart
-- with lua, you can only use variables/functions you've defined above this
-- so you probably want this at the bottom of the file
function TwiCutlass(lthing)
	-- the 0 means we want the function to run when the 0th (i.e. the first) sound component is updated
	addSoundFrameFunction(lthing, 0, engine)
end