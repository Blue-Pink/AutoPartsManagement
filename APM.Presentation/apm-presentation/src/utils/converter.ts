import dayjs from "dayjs";

/**
 * 将 UTC DateTime 字符串转换为指定格式的日期字符串
 * @param dateString UTC DateTime 字符串 (例如: 2026-04-21T10:30:45.000Z)
 * @param format 格式模式 (例如: yyyy-mm-dd hh:mm:ss)
 * @returns 格式化后的日期字符串
 */
export function ConvertDateTime(dateString: string, format: string = 'yyyy-mm-dd hh:mm:ss'): string {
    if (!dateString) {
        return '-'
    }

    try {
        //将UTC字符串转换成Date对象, 且转换成UTC+8时间
        const date = new Date(dateString)
        date.setHours(date.getHours() + 8)
        // 检查日期是否有效
        if (isNaN(date.getTime())) {
            return '-'
        }

        const year = date.getFullYear()
        const month = String(date.getMonth() + 1).padStart(2, '0')
        const day = String(date.getDate()).padStart(2, '0')
        const hours = String(date.getHours()).padStart(2, '0')
        const minutes = String(date.getMinutes()).padStart(2, '0')
        const seconds = String(date.getSeconds()).padStart(2, '0')

        // 使用临时占位符避免 mm 的歧义（月份 vs 分钟）
        let result = format.toLowerCase()

        // 第一步：使用占位符替换所有值
        result = result
            .replace(/yyyy/g, '__YYYY__')
            .replace(/-mm-/g, '-__MM__-')  // 月份通常被-包围
            .replace(/hh/g, '__HH__')
            .replace(/mm/g, '__MIN__')      // 剩余的 mm 当作分钟
            .replace(/dd/g, '__DD__')
            .replace(/ss/g, '__SS__')

        // 第二步：用实际值替换占位符
        result = result
            .replace(/__YYYY__/g, String(year))
            .replace(/__MM__/g, month)
            .replace(/__DD__/g, day)
            .replace(/__HH__/g, hours)
            .replace(/__MIN__/g, minutes)
            .replace(/__SS__/g, seconds)

        return result
    } catch (error) {
        console.warn('日期格式化失败:', error)
        return '-'
    }
}

/**
 * 格式化日期时间
 * @param date 原始日期数据 (Date 对象, 字符串, 或时间戳)
 * @param pattern 格式模板，默认 YYYY-MM-DD
 */
export const FormatDate = (
    date: Date | string | number | undefined | null,
    pattern: string = 'YYYY-MM-DD'
): string => {
    if (!date) return '';
    return dayjs(date).format(pattern);
};

/**
 * 针对汽配业务的特殊转换：例如将毫秒转为天数（用于超期存货计算）
 */
export const daysDiff = (startDate: Date, endDate: Date): number => {
    return dayjs(endDate).diff(dayjs(startDate), 'day');
};
