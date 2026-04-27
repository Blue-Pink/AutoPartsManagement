import $ from '@/utils/request';
import type { UsualApiData } from '@/interfaces/HttpReponse';

class UsualEntityService {
    Get<T>(entityName: string, id: string): Promise<UsualApiData<T>> {
        return $.get(`Entity/Get/${entityName}/${id}`);
    }
}

export default new UsualEntityService();