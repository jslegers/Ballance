return {
  ---模块入口函数
  ---@param thisGamePackage GamePackage
  ---@return boolean
  PackageEntry = function(thisGamePackage)
    thisGamePackage:RequireLuaFile("InitCore")
    CoreInit()
    return true
  end,
  ---模块卸载前函数
  ---@param thisGamePackage GamePackage
  ---@return boolean
  PackageBeforeUnLoad = function(thisGamePackage)
    CoreUnload()
    return true
  end
}
