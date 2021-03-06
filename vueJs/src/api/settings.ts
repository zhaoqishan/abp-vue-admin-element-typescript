import { ListResultDto } from './types'
import ApiService from './serviceBase'

const IdentityService = process.env.VUE_APP_BASE_API

export default class SettingApiService {
  /**
   * 获取公共配置
   */
  public static getGlobalSettings() {
    const _url = '/api/setting-management/settings/by-global'
    return ApiService.Get<ListResultDto<Setting>>(_url, IdentityService)
  }

  /**
   * 设置公共配置
   */
  public static setGlobalSettings(payload: SettingsUpdate) {
    const _url = '/api/setting-management/settings/by-global'
    return ApiService.Put<any>(_url, payload, IdentityService)
  }

  /**
   * 获取当前租户配置
   */
  public static getCurrentTenantSettings() {
    const _url = '/api/setting-management/settings/by-current-tenant'
    return ApiService.Get<ListResultDto<Setting>>(_url, IdentityService)
  }

  /**
   * 设置当前租户配置
   */
  public static setCurrentTenantSettings(payload: SettingsUpdate) {
    const _url = '/api/setting-management/settings/by-current-tenant'
    return ApiService.Put<any>(_url, payload, IdentityService)
  }

  /**
   * 获取当前用户配置
   */
  public static getCurrentUserSettings() {
    const _url = '/api/setting-management/settings/by-current-user'
    return ApiService.Get<ListResultDto<Setting>>(_url, IdentityService)
  }

  /**
   * 设置当前用户配置
   */
  public static setCurrentUserSettings(payload: SettingsUpdate) {
    const _url = '/api/setting-management/settings/by-current-user'
    return ApiService.Put<any>(_url, payload, IdentityService)
  }

  /**
   * 获取用户配置
   */
  public static getUserSettings(userId: string) {
    const _url = '/api/setting-management/settings/by-user/' + userId
    return ApiService.Get<ListResultDto<Setting>>(_url, IdentityService)
  }

  /**
   * 设置当前用户配置
   */
  public static setUserSettings(userId: string, payload: SettingsUpdate) {
    const _url = '/api/setting-management/settings/by-user/' + userId
    return ApiService.Put<any>(_url, payload, IdentityService)
  }

  /**
   * 保存配置
   * @param providerName  配置提供者名称
   * @param providerKey   配置提供者标识
   * @param payload       配置变更信息
   * @returns Promise对象
   */
  public static setSettings(providerName: string, providerKey: string, payload: SettingsUpdate) {
    let _url = '/api/settings'
    _url += '?providerName=' + providerName
    _url += '&providerKey=' + providerKey
    return ApiService.Put<any>(_url, payload, IdentityService)
  }
}

export class SettingBase {
  /** 名称 */
  name!: string
  /** 当前设置值 */
  value!: any
}

/** 设置对象 */
export class Setting extends SettingBase {
  /** 显示名称 */
  displayName!: string
  /** 说明 */
  description!: string
  /** 默认设置 */
  defaultValue!: string

  public getValue() {
    if (this.value) {
      return this.value
    }
    return this.defaultValue
  }
}

/** 配置变更对象 */
export class SettingUpdate extends SettingBase {}

/** 配置变更集合对象 */
export class SettingsUpdate {
  /** 配置集合 */
  settings: SettingUpdate[]

  constructor() {
    this.settings = new Array<SettingUpdate>()
  }
}
