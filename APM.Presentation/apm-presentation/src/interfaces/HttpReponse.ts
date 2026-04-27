export interface UsualApiData<T> {
    data: T | null;
    dataList: T[] | null;
    message: string | null;
    customData: object | null;
    stateCode: number;
    pageIndex?: number;
    pageSize?: number;
    total?: number;
}