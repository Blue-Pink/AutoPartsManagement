import { FormatDate } from './converter';

/**
 * 常量字典
 * 使用 get 语法实现动态属性，确保每次获取都是当前系统时间
 */
export const ConstDictionary = {
    // 获取当前日期字符串 (例如: 2026-05-04)
    get CURRENT_DATE() {
        return FormatDate(new Date(), 'YYYY-MM-DD');
    },

    // 获取当前详细时间字符串 (例如: 2026-05-04 14:30:05)
    get CURRENT_DATETIME() {
        return FormatDate(new Date(), 'YYYY-MM-DD HH:mm:ss');
    },

    get EMPTY_GUID() {
        return '00000000-0000-0000-0000-000000000000';
    },

    get TABLE_PAGE_SIZES() {
        return [25, 50, 100];
    },

    get CHILD_TABLE_PAGE_SIZES() {
        return [5, 10, 20];
    },
};