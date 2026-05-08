import { dayjs } from 'element-plus'
import utc from 'dayjs/plugin/utc'
import timezone from 'dayjs/plugin/timezone'

dayjs.extend(utc)
dayjs.extend(timezone)

const defaultTimezone = dayjs.tz.guess() || 'UTC'
dayjs.tz.setDefault(defaultTimezone)
const defaultFormat = 'YYYY-MM-DD HH:mm:ss'
/**
 * 将 UTC DateTime 字符串转换为指定格式的日期字符串
 * @param dateString UTC DateTime 字符串 (例如: 2026-04-21T10:30:45.000Z)
 * @param format 格式模式 (例如: YYYY-MM-DD HH:mm:ss)n
 * @returns 格式化后的日期字符串
 */
export const ConvertDateTime = (dateString: string, format: string = ''): string => {
    if (!dateString)
        return ''

    try {
        return dayjs.utc(dateString).tz(defaultTimezone).format(format || defaultFormat)
    } catch (error) {
        console.warn('日期格式化失败:', error)
        return ''
    }
}

/**
 * 格式化日期时间
 * @param date 原始日期数据 (Date 对象, 字符串, 或时间戳)
 * @param format 格式模板，默认 YYYY-MM-DD
 */
export const FormatDate = (
    date: Date | string | number | undefined | null, format: string = ''
): string => {
    if (!date) return '';

    if (typeof date === 'string' && date.endsWith('Z'))
        return dayjs.utc(date).tz(defaultTimezone).format(format || defaultFormat);

    return dayjs.utc(date).tz(defaultTimezone).format(format || defaultFormat);
};

export const Now = (format: string = ''): string => {
    return dayjs.utc().tz(defaultTimezone).format(format || defaultFormat)
}

export const DaysDiff = (startDate: Date, endDate: Date): number => {
    return dayjs(endDate).diff(dayjs(startDate), 'day');
};

export const NumberToFixed = (num: number, decimalPlaces: number = 2): number => {
    return Number(num.toFixed(decimalPlaces));
}
