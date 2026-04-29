import $ from '@/utils/requestor';
import type { UsualApiData } from '@/interfaces/HttpReponse';

class UsualEntityService {
    Get<T>(entityName: string, id: string): Promise<UsualApiData<T>> {
        return $.get(`Entity/Get/${entityName}/${id}`);
    }

    AutoNumber(entityName: string, prefix: string, digits: number = 4): Promise<UsualApiData<string>> {
        return $.get(`Entity/AutoNumber?entityName=${encodeURIComponent(entityName)}&prefix=${encodeURIComponent(prefix)}&digits=${digits}`);
    }

    Delete(entityName: string, ids: string[]): Promise<UsualApiData<number>> {
        return $.delete(`Entity/Delete/${entityName}`, { data: ids });
    }
}

export default new UsualEntityService();