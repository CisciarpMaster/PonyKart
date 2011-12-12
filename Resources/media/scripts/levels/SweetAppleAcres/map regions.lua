-- only run this part if our model detail level is set to "Medium"
-- a high model detail setting doesn't want imposters
if getOption("ModelDetail") == "Medium" then

	local player = playerKart()
	local currentRegion

	local function show(regionName)
		setInstancedGeometryVisibility(regionName, true)
		setStaticGeometryVisibility(regionName, true)
		setRegionNodeVisibility(regionName, true)
		
		setImposterVisibility(regionName, false)
	end

	local function hide(regionName)
		setInstancedGeometryVisibility(regionName, false)
		setStaticGeometryVisibility(regionName, false)
		setRegionNodeVisibility(regionName, false)
		
		setImposterVisibility(regionName, true)
	end

	-- returns whether the given shape is the player's kart, since we only want the player to be affecting geometry
	local function checkPlayer(shape)
		return player.Body == shape
	end

	-------------------------------------------

	-- show buckets when entering, show bridge when leaving
	local function startTriggerRegion(region, shape, flags)
		if checkPlayer(shape) and currentRegion ~= "Start" then
			currentRegion = "Start"
		
			if isEnterFlag(flags) then
				show("Start")
				show("Cliff")
				show("Buckets")
				hide("Ramp")
				hide("River")
				hide("Treehouse")
				hide("Bridge")
				show("Barn")
			end
		end
	end

	-- show ramp when entering, hide barn and bridge when leaving
	local function cliffTriggerRegion(region, shape, flags)
		if checkPlayer(shape) and currentRegion ~= "Cliff" then
			currentRegion = "Cliff"
		
			if isEnterFlag(flags) then
				show("Start")
				show("Cliff")
				show("Buckets")
				show("Ramp")
				hide("River")
				hide("Treehouse")
				show("Bridge")
				show("Barn")
			end
		end
	end

	-- show river when entering, hide start when leaving
	local function bucketsTriggerRegion(region, shape, flags)
		if checkPlayer(shape) and currentRegion ~= "Buckets" then
			currentRegion = "Buckets"
		
			if isEnterFlag(flags) then
				show("Start")
				show("Cliff")
				show("Buckets")
				show("Ramp")
				show("River")
				hide("Treehouse")
				hide("Bridge")
				hide("Barn")
			end
		end
	end

	-- show treehouse when entering, hide cliff when leaving
	local function rampTriggerRegion(region, shape, flags)
		if checkPlayer(shape) and currentRegion ~= "Ramp" then
			currentRegion = "Ramp"
		
			if isEnterFlag(flags) then
				hide("Start")
				hide("Cliff")
				show("Buckets")
				show("Ramp")
				show("River")
				show("Treehouse")
				hide("Bridge")
				hide("Barn")
			end
		end
	end

	-- hide buckets when entering, hide ramp when leaving
	local function riverTriggerRegion(region, shape, flags)
		if checkPlayer(shape) and currentRegion ~= "River" then
			currentRegion = "River"
			
			if isEnterFlag(flags) then
				hide("Start")
				hide("Cliff")
				hide("Buckets")
				show("Ramp")
				show("River")
				show("Treehouse")
				hide("Bridge")
				hide("Barn")
			end
		end
	end

	-- show bridge when entering, hide river when leaving
	local function treehouseTriggerRegion(region, shape, flags)
		if checkPlayer(shape) and currentRegion ~= "Treehouse" then
			currentRegion = "Treehouse"
			
			if isEnterFlag(flags) then
				hide("Start")
				hide("Cliff")
				hide("Buckets")
				hide("Ramp")
				show("River")
				show("Treehouse")
				show("Bridge")
				hide("Barn")
			end
		end
	end

	-- show barn when entering, hide treehouse when leaving
	local function bridgeTriggerRegion(region, shape, flags)
		if checkPlayer(shape) and currentRegion ~= "Bridge" then
			currentRegion = "Bridge"
		
			if isEnterFlag(flags) then
				hide("Start")
				hide("Cliff")
				hide("Buckets")
				hide("Ramp")
				hide("River")
				show("Treehouse")
				show("Bridge")
				show("Barn")
			end
		end
	end

	-- show start and cliff when entering, hide bridge when leaving
	local function barnTriggerRegion(region, shape, flags)
		if checkPlayer(shape) and currentRegion ~= "Barn" then
			currentRegion = "Barn"
			
			if isEnterFlag(flags) then
				show("Start")
				show("Cliff")
				hide("Buckets")
				hide("Ramp")
				hide("River")
				hide("Treehouse")
				show("Bridge")
				show("Barn")
			end
		end
	end

	-- none of our regions are rotated, so they all get the identity matrix
	local iden = quat(0, 0, 0, 1)

	-- create our regions!
	createBoxTriggerRegion("startTriggerRegion", startTriggerRegion, vec(128.4655, 55.3625, 60.738), vec(-180.371, 43.273, 319.494), iden)
	createBoxTriggerRegion("cliffTriggerRegion", cliffTriggerRegion, vec(65.189, 70.6485, 211.1895), vec(-318.423, 70.648, 75.149), iden)
	createBoxTriggerRegion("bucketsTriggerRegion", bucketsTriggerRegion, vec(124.403, 63.056, 200.649), vec(-336.431, 52.028, -339.59), iden)
	createBoxTriggerRegion("rampTriggerRegion", rampTriggerRegion, vec(168.823, 66.583, 155.504), vec(-242.448, 66.583, -696.777), iden)
	createBoxTriggerRegion("riverTriggerRegion", riverTriggerRegion, vec(163.3765, 68.951, 157.602), vec(93.109, 56.093, -694.3), iden)
	createBoxTriggerRegion("treehouseTriggerRegion", treehouseTriggerRegion, vec(100.9415, 72.3965, 237.368), vec(348.96, 72.397, -450.379), iden)
	createBoxTriggerRegion("bridgeTriggerRegion", bridgeTriggerRegion, vec(178.2205, 54.084, 156.443), vec(149.331, 40.06, -53.333), iden)
	createBoxTriggerRegion("barnTriggerRegion", barnTriggerRegion, vec(153.9025, 44.6625, 153.9025), vec(101.665, -33.559, 257.315), iden)

	-- hide some things when we start up
	hide("Bridge")
	hide("Treehouse")
	hide("River")
	hide("Ramp")
	hide("Buckets")

-- a low model detail setting wants imposters and no static geometry
elseif getOption("ModelDetail") == "Low" then

	setImposterVisibility("Start", true)
	setImposterVisibility("Cliff", true)
	setImposterVisibility("Buckets", true)
	setImposterVisibility("Ramp", true)
	setImposterVisibility("River", true)
	setImposterVisibility("Treehouse", true)
	setImposterVisibility("Bridge", true)
	setImposterVisibility("Barn", true)
end