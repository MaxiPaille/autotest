namespace Autotest.Internal
{
    
    internal static class LuaUtils
    {

        public static readonly string dump = "function Dump(a)if type(a)=='table'then local b='{ 'for c,d in pairs(a)do if type(c)=='userdata'then b=b..'userdata'..' = 'else b=b..'['..c..'] = 'end;b=b..Dump(d)..', 'end;return b..'} 'elseif type(a)=='userdata'then return'userdata'else return tostring(a)end end";
        // function Dump(object)
        //     if type(object) == 'table' then
        //         local message = '{ '
        //     
        //         for key, value in pairs(object) do
        //             if type(key) == 'userdata' then 
        //                 message = message .. 'userdata' .. ' = '
        //             else
        //                message = message .. '[' .. key .. '] = '
        //             end
        //             message = message .. Dump(value) .. ', '
        //         end
        //         return message .. '} '
        //     elseif type(object) == 'userdata' then
        //         return 'userdata'
        //     else
        //         return tostring(object)
        //     end
        // end

    }
    
}
