using System.Collections.Generic;
using UnityEngine;

public static class CooldownManager
{
    private static Dictionary<object, float> _cooldowns = new Dictionary<object, float>();
    
    public static void StartCooldown(object obj, float cooldownDuration)
    {
        SetValue(obj, Time.time + cooldownDuration);
    }

    public static bool IsCooldownDone(object obj)
    {
        if (_cooldowns.ContainsKey(obj))
            return Time.time > _cooldowns[obj];
        
        return true; // not started cooldowns are done by default
    }

    private static void SetValue(object key, float value)
    {
        if (_cooldowns.ContainsKey(key))
            _cooldowns[key] = value;
        else
            _cooldowns.Add(key, value);
    }

}
