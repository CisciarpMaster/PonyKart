local threshold = 30 / 2 --45

local region0, region1, region2, region3, region3a, region4, region5, region6, region7, region8, region9, region10, region11

-- if it's a kart with a computer player, this tells it the next waypoint to go to
local function goTo(currentRegion, nextRegion, body, info)
	local k = getKartFromBody(body)
	if k ~= nil then
		if k.Player.IsComputerControlled then
			k.Player:CalculateNewWaypoint(currentRegion, nextRegion, info)
		end
	end
end

-- due to how trigger regions work, this function is only ever ran when the entering body is in the kart collision group
local function tr0(region, body, flags, info)  goTo(region, region1, body, info) end
local function tr1(region, body, flags, info)  goTo(region, region2, body, info) end
local function tr2(region, body, flags, info)  goTo(region, region3, body, info) end
local function tr3(region, body, flags, info)  goTo(region, region4, body, info) end
local function tr3a(region, body, flags, info) goTo(region, region4, body, info) end
local function tr4(region, body, flags, info)  goTo(region, region5, body, info) end
local function tr5(region, body, flags, info)  goTo(region, region6, body, info) end
local function tr6(region, body, flags, info)  goTo(region, region7, body, info) end
local function tr7(region, body, flags, info)  goTo(region, region8, body, info) end
local function tr8(region, body, flags, info)  goTo(region, region9, body, info) end
local function tr9(region, body, flags, info)  goTo(region, region10, body, info) end
local function tr10(region, body, flags, info) goTo(region, region11, body, info) end
local function tr11(region, body, flags, info) goTo(region, region0, body, info) end

function TestAI(level)
	local iden = quat(0, 0, 0, 1)
	
	-- need to divide the dimensions you get from 3ds max by two if you start with a 1x1x1 box...
	-- use a 2x2x2 box in the future?
	-- oh and remember to make sure you do "align to world" to the first one before you start, otherwise you'll have to mirror them across the X axis (negate the Y and Z parts of the quaternion)
	region0 = createBoxTriggerRegion("TriggerRegion0", tr0, vec(67.3105, 27.48, threshold), vec(4.08081, 27.2801, 17.4568), iden)
	region1 = createBoxTriggerRegion("TriggerRegion1", tr1, vec(67.3105, 27.48, threshold), vec(4.08081, 27.2801, 152.999), iden)
	region2 = createBoxTriggerRegion("TriggerRegion2", tr2, vec(64.8625, 27.48, threshold), vec(-19.2979, 27.2801, 200.328), quat(0, 0.382683, -1.67276E-08, 0.92388))
	region3 = createBoxTriggerRegion("TriggerRegion3", tr3, vec(46.2213, 27.48, threshold), vec(-105.629, 27.2801, 244.241), quat(0, 0.707107, -3.09086E-08, 0.707107))
	region3a = createBoxTriggerRegion("TriggerRegion3a", tr3a, vec(67.333, 27.48, 22.4699), vec(-84.4353, 27.2801, 313.466), iden)
	region4 = createBoxTriggerRegion("TriggerRegion4", tr4, vec(49.8211, 27.48, threshold), vec(-174.615, 27.2801, 199.13), quat(0, 0.843391, -3.68658E-08, 0.5373))
	region5 = createBoxTriggerRegion("TriggerRegion5", tr5, vec(63.644, 27.48, threshold), vec(-218.611, 27.2801, 176.994), quat(0, -0.923879, 4.03841E-08, -0.382684))
	region6 = createBoxTriggerRegion("TriggerRegion6", tr6, vec(50.5025, 27.48, threshold), vec(-241.512, 27.2801, 129.989), quat(0, -0.965926, 4.2222E-08, -0.258819))
	region7 = createBoxTriggerRegion("TriggerRegion7", tr7, vec(66.8435, 27.48, threshold), vec(-263.01, 27.2801, 100.682), quat(0, -1, 4.37114E-08, -1.94707E-07))
	region8 = createBoxTriggerRegion("TriggerRegion8", tr8, vec(44.9004, 27.48, threshold), vec(-240.565, 27.2801, 15.8291), quat(0, -1, 4.37114E-08, -1.94707E-07))
	region9 = createBoxTriggerRegion("TriggerRegion9", tr9, vec(44.9004, 27.48, threshold), vec(-196.24, 27.2801, -29.3041), quat(0, -0.707107, 3.09086E-08, 0.707107))
	region10 = createBoxTriggerRegion("TriggerRegion10", tr10, vec(69.8035, 27.48, threshold), vec(-151.084, 27.2801, -52.27), quat(2.51215E-15, -0.707107, 3.09086E-08, 0.707107))
	region11 = createBoxTriggerRegion("TriggerRegion11", tr11, vec(63.6775, 27.48, threshold), vec(-17.3832, 27.2801, -29.3153), quat(0, -0.382684, 1.67276E-08, 0.923879))
	
	-- decrease the kart speed so we can watch them better
	function decreaseKartSpeed(kart, id)
		kart.MaxSpeed = kart.MaxSpeed * 0.5
	end
	
	-- give all of the computer-controlled karts an initial region to go to
	function setInitialRegion(kart, id)
		if kart.Player.IsComputerControlled then
			kart.Player:CalculateNewWaypoint(nil, region0, nil)
		end
	end

	forEachKart(decreaseKartSpeed)
	forEachKart(setInitialRegion)
end