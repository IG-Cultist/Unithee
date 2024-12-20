/*
 * SelectSceneScript
 * Creator:���Y�W�� Update:2024/11/07
*/
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Rendering;
public class SelectScene : MonoBehaviour
{
    // �X�e�[�W���
    [SerializeField] GameObject info;

    // �X�e�[�W�{�^��
    [SerializeField] GameObject btnPrefab;

    // �����{�^��
    [SerializeField] GameObject infoButton;

    // Card Explain Text
    [SerializeField] Text infoTxt;

    // �f�b�L��ʃe�L�X�g
    [SerializeField] Text deckTxt;

    // �x���e�L�X�g
    [SerializeField] Text warning;

    // �X�e�[�W�̐e
    [SerializeField] GameObject stageParent;

    // �J�[�h�̐e
    [SerializeField] GameObject cardParent;

    // �X�e�[�W����p�l��
    [SerializeField] GameObject infoPanel;

    // �f�b�L�\�z�p�l��
    [SerializeField] GameObject deckBuildPanel;

    // �J�[�h�Q�ƃp�l��
    [SerializeField] GameObject showCardPanel;

    // �r���h�p�l��
    [SerializeField] GameObject buildPanel;

    // �J�[�h�p�l��
    [SerializeField] GameObject cardViewPanel;

    // �A�C�R���p�l��
    [SerializeField] GameObject iconPanel;

    // �ڍ׃p�l��
    [SerializeField] GameObject helpPanel;

    // �J�[�h�e
    [SerializeField] GameObject cardViewParent;

    // ���݂̃f�b�L�̐e
    [SerializeField] GameObject activeDeckParent;

    // ���݂̖h�q�f�b�L�̐e
    [SerializeField] GameObject activeDefenceDeckParent;

    //���C���f�b�L�̐e
    [SerializeField] GameObject mainDeckPanel;

    // �h�q�f�b�L�̐e
    [SerializeField] GameObject defenceDeckPanel;

    // ���[�f�B���O�p�l��
    [SerializeField] GameObject loadingPanel;

    // ���[�f�B���O�A�C�R��
    [SerializeField] GameObject loadingIcon;

    // Deck Panel
    [SerializeField] Text infoText;

    // �v���C���[��
    [SerializeField] Text playerName;

    // Clear's SoundEffect
    AudioClip clickSE;

    public bool isClick = false;
    bool isSet;

    // �f�b�L�f�[�^�p�X�N���v�g
    DeckData deckData;

    AudioSource audioSource;

    GameObject nowIcon;

    Image iconImage;
    int iconNum;
    string iconName = "icon000";


    // Start is called before the first frame update
    void Start()
    {
        // �񓯊����������܂őҋ@������
        Loading();
        isSet = false;

        // SetSE
        this.gameObject.AddComponent<AudioSource>();
        audioSource = GetComponent<AudioSource>();

        clickSE = (AudioClip)Resources.Load("SE/Click");

        // �S�Ẵp�l�������
        infoPanel.SetActive(false);
        info.SetActive(false);
        deckBuildPanel.SetActive(false);
        showCardPanel.SetActive(false);
        defenceDeckPanel.SetActive(false);
        helpPanel.SetActive(false);
        iconPanel.SetActive(false);

        // �I�u�W�F�N�g�̎擾
        nowIcon = GameObject.Find("MyIcon");

        // �R���|�[�l���g�̎擾
        iconImage = nowIcon.GetComponent<Image>();

        // �f�b�L�f�[�^�X�N���v�g���擾
        deckData = FindObjectOfType<DeckData>();

        // �X�e�[�W���擾
        NetworkManager networkManager = NetworkManager.Instance;
        //if (networkManager.displayName == "")
        //{
        //    playerName.text = "���ݒ�";
        //}else playerName.text = networkManager.displayName;

        //if (networkManager.iconName != "")
        //{
        //    Texture2D texture = Resources.Load("Icons/" + networkManager.iconName) as Texture2D;

        //    iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
        //                                   Vector2.zero);
        //}

        List<int> stageIDs = networkManager.GetID();
        StartCoroutine(NetworkManager.Instance.GetStage(stages =>
        {
            int cnt = 0;
            foreach (var stage in stages)
            {
                // Create Stage Button from Server
                GameObject btn = new GameObject();
                Destroy(btn);
                if (cnt < 5)
                {
                    btn = Instantiate(btnPrefab, new Vector2(-550 + (150 * cnt), 300), Quaternion.identity);
                }
                else
                {
                    btn = Instantiate(btnPrefab, new Vector2(-550 + (150 * (cnt - 5)), 150), Quaternion.identity);
                }

                // Rename for StageID
                btn.name =/*"Stage" + */ stage.StageID.ToString();

                //Change Button's Text for StageID
                Transform btnText = btn.transform.Find("Text");
                btnText.gameObject.GetComponent<Text>().text = stage.StageID.ToString();

                // Add Button in Canvas
                string btnName = btn.name;
                btn.transform.SetParent(this.stageParent.transform, false);
                btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => selectStage(btnName));

                if (stageIDs != null && stageIDs.Contains(stage.StageID))
                {
                    btn.GetComponent<Image>().color = Color.yellow;
                }
                cnt++;
            }
        }));

        StartCoroutine(NetworkManager.Instance.GetMyProfile(user =>
        {
            if (user.Length == 0)
            {
                StartCoroutine(NetworkManager.Instance.StoreProfile());
            }
            else
            {
                foreach (var profile in user)
                {
                    if (profile.Name != "")
                    {
                        playerName.text = profile.Name;
                    }

                    if (profile.IconName != "")
                    {
                        Texture2D texture = Resources.Load("Icons/" + profile.IconName) as Texture2D;

                        iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                                       Vector2.zero);
                    }
                }
            }
        }));
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0)) audioSource.PlayOneShot(clickSE);

        if (Input.GetMouseButtonUp(1)) CheckSomething();
    }

    /// <summary>
    /// Open Info Panel
    /// </summary>
    /// <param name="btnName"></param>
    public void selectStage(string btnName)
    {
        // Change Info's StageID
        Transform infoText = info.transform.Find("Text");
        infoText.gameObject.GetComponent<Text>().text = btnName;

        // Info�p�l���݂̂��J���A����ȊO�����
        info.SetActive(true);
        deckBuildPanel.SetActive(false);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);
    }

    /// <summary>
    /// �f�b�L�ǉ�����
    /// </summary>
    void AddDeck(GameObject obj)
    {
        warning.text = "";
        List<int> activeList;
        List<string> otherList;
        int requestValue;
        if (isClick == false)
        {
            activeList = deckData.GetDeck();
            otherList = deckData.GetDefenceDeckName();
            requestValue = 1;
        }
        else
        {
            activeList = deckData.GetDefenceDeck();
            otherList = deckData.GetDeckName();
            requestValue = 2;
        }

        // �I�����ꂽ�I�u�W�F�N�g�ԍ���int�ɂ���
        int cardID = deckData.ConvertName(obj.name);
        
        // �ΏۃI�u�W�F�N�g�̏�Ԃ��m�F
        bool isSelected = deckData.CheckUsable(obj.name);

        if (otherList.Contains(obj.name))
        {
            warning.text = "���̃J�[�h�͕ʂ̃f�b�L�Ɋ܂܂�Ă���I";
        }
        else if (isSelected == true)
        { // �I������Ă����ꍇ
            // ��Ԃ𖢑I���ɂ���
            deckData.UpdateUsable(obj.name, 0);
            // �I���J�[�hID���X�g���珜��
            activeList.Remove(cardID);
        }
        else if(isSelected == false && activeList.Count < 4) 
        { // �ΏۃI�u�W�F�N�g�����I�������݂̃f�b�L����4�����̏ꍇ
            // �I����Ԃɂ���
            deckData.UpdateUsable(obj.name, requestValue);
            // �I���J�[�hID���X�g�ɒǉ�
            activeList.Add(cardID);
        }
        else if (activeList.Count == 4)
        {
            warning.text = "����ȏ�͒ǉ��ł��Ȃ��I";
        }
        // �X�N���[���r���[���X�V
        UpdaetView(obj);
    }

    /// <summary>
    /// �J�[�h�ꗗ�X�V����
    /// </summary>
    void UpdaetView(GameObject obj)
    {
        List<List<int>> usableCards = deckData.GetUsable();
        for (int i = 1; i <= 9; i++)
        {
            // 4�񃋁[�v
            for (int j = 1; j <= 4; j++)
            {
                // �����񔻒�
                if (obj.name == i.ToString() + "," + j.ToString())
                {
                    // �I������Ă���Ȃ�F��ύX
                    if (usableCards[i - 1][j - 1] == 1)
                    {
                        obj.GetComponent<Image>().color = Color.gray;
                    }
                    else if (usableCards[i - 1][j - 1] == 2)
                    {
                        obj.GetComponent<Image>().color =Color.blue;
                    }
                    else
                    { //�F�����Z�b�g
                        obj.GetComponent<Image>().color = Color.white;
                    }
                }
            }
        }
    }

    /// <summary>
    /// �f�b�L�\������
    /// </summary>
    public void UpdateDeck(List<int> id)
    {
        GameObject parent;
        if (isClick == false)
        {
            parent = activeDeckParent;
        }
        else
        {
            parent = activeDefenceDeckParent;
        }

        foreach (Transform n in parent.transform)
        {
            GameObject.Destroy(n.gameObject);
        }

        // �e�J�[�h�̃X�^�b�N�������[�v
        for (int i = 0; i < id.Count; i++)
        {
            if (id[i] == 0) continue;
            // �����̃J�[�h�����\�[�X�t�@�C������擾
            GameObject obj = (GameObject)Resources.Load("Cards(ID)/" + id[i]);
            // �擾�����J�[�h�𐶐�
            GameObject cards = Instantiate(obj, new Vector2(-430f + (280f * i), 0f), Quaternion.identity);

            cards.name = obj.name;
            cards.transform.localScale = new Vector2(1.7f,2.4f);
            // ���C���f�b�L�p�l���ɐ���
            cards.transform.SetParent(parent.transform, false);
        }
    }

    /// <summary>
    /// �����_���l�[���R���o�[�g����
    /// </summary>
    public void randomName()
    {   
        System.Random rand = new System.Random();
        // �t�@�X�g�l�[����`
        string[] firstName = new string[]{
            "Nice","Abnormal","Delicious","Difficulty","Mr",
            "Mrs","Master","Huge","Tiny","Clever",
            "Wetty","Pretty","Golden","Brave","Godly",
            "Kidly","Burning","Creepy","Fishy","Metallic",
            "Oriental","Muscly","Mudly","More","Strong",
            "Shiny","Sparkle","Legal","Hardest","Dancing"
        };
        // �Z�J���h�l�[����`
        string[] secondtName = new string[]{
            "Cake","Rock","Slime","Clover","Animal",
            "Fish","Earth","Throat","City","Dwarf",
            "Ghost","Tank","Knight","Candy","Worm",
            "Tree","Dice","Baby","Machine","Dog",
            "Thief","Bird","Cat","Water","CowBoy",
            "Skelton","Boots","Game","Card","Data"
        };
        // 1�`30�܂ł̗�������
        int num = rand.Next(1, 30);
        int num2 = rand.Next(1, 30);

        // �e�����ɉ��������O����
        playerName.text ="Name:" + firstName[num] + secondtName[num2];
    }

    void CheckSomething()
    {
        List<List<int>> usableCards = deckData.GetUsable();

        Debug.Log(usableCards[1][2]);
    }

    /// <summary>
    /// �f�b�L�؂�ւ�����
    /// </summary>
    public void ChangeDeck()
    {
        if (isClick == false)
        {
            mainDeckPanel.SetActive(false);
            defenceDeckPanel.SetActive(true);
            deckTxt.text = "Defence Deck";
            isClick = true;

            // �h�q�f�b�L���X�V���A�\��
            UpdateDeck(deckData.GetDefenceDeck());
        }
        else
        {
            mainDeckPanel.SetActive(true);
            defenceDeckPanel.SetActive(false);
            deckTxt.text = "Main Deck";
            isClick = false;   

            // ���C���f�b�L���X�V���A�\��
            UpdateDeck(deckData.GetDeck());
        }
    }

    /// <summary>
    /// �J�[�h�ꗗ��������
    /// </summary>
    void CardSet()
    {
        if (isSet == true) return;
        int cnt = 0;
        List<List<int>> usableCards =  deckData.GetUsable();
        // �f�B�N�V���i���[���̃A�C�e�������[�v
        foreach (var item in deckData.cardDictionary.Keys)
        {
            // �L�[�𕶎���ɕϊ�
            string cardName = item.ToString();
            // �X�^�b�N���𐔎��ɕϊ�
            int.TryParse(deckData.cardDictionary[cardName].Stack, out int stack);

            // �e�J�[�h�̃X�^�b�N�������[�v
            for (int i = 0; i < 4; i++)
            {
                // ���[�v�����X�^�b�N�������̏ꍇ
                if (stack > i)
                {
                    // �����̃J�[�h�����\�[�X�t�@�C������擾
                    GameObject obj = (GameObject)Resources.Load("UI/" + cardName);
                    // �擾�����J�[�h�𐶐�
                    GameObject cards = Instantiate(obj, new Vector2(-400f + (200f * i), 125f - (250f * cnt)), Quaternion.identity);
                    // Rename
                    cards.name = (cnt+1) + "," + (i+1);
                    if (usableCards[cnt][i] == 1)
                    {
                        cards.GetComponent<Image>().color = Color.gray;
                    }
                    if (usableCards[cnt][i] == 2)
                    {
                        cards.GetComponent<Image>().color = Color.blue;
                    }
                    cards.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => AddDeck(cards));

                    // �����J�[�h���X�N���[���r���[�ɒǉ�
                    cards.transform.SetParent(cardViewParent.transform, false);
                }
                else // ���[�v�����X�^�b�N���𒴂����ꍇ�A�_�~�[�𐶐����Đ��ڂ���
                {
                    // �����ȃ_�~�[�����\�[�X�t�@�C������擾
                    GameObject obj = (GameObject)Resources.Load("UI/Dummy");
                    // �擾�����_�~�[�𐶐�
                    GameObject cards = Instantiate(obj, new Vector2(-400f + (200f * i), 125f - (250f * cnt)), Quaternion.identity);
                    // Rename
                    cards.name = "dummy";

                    // �_�~�[���X�N���[���r���[�ɒǉ�
                    cards.transform.SetParent(cardViewParent.transform, false);
                }
            }
            cnt++;
        }
        isSet = true;
    }

    /// <summary>
    /// Close Info Panel
    /// </summary>
    public void exitInfo()
    {
        info.SetActive(false);
    }

    /// <summary>
    /// Load Some Stage Scene
    /// </summary>
    public void startScene()
    {
        Transform infoText = info.transform.Find("Text");
        SceneManager.LoadScene(infoText.gameObject.GetComponent<Text>().text);
    }

    /// <summary>
    /// Load Tutprial Scene
    /// </summary>
    public void Tutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    /// <summary>
    /// Open Stage Info Panel
    /// </summary>
    public void stageInfo()
    {
        deckBuildPanel.SetActive(false);
        infoPanel.SetActive(true);
        Transform stageInfo = info.transform.Find("Text");

        switch (stageInfo.gameObject.GetComponent<Text>().text)
        {
            case "1":
                infoTxt.text = "�������菇�Ō��j��ڎw����";
                break;

            case "2":
                infoTxt.text = "�����ł̓V�[���h��j�󂷂邱�Ƃ��d�v�ƂȂ�\n";
                break;

            case "3":
                infoTxt.text = "�����������Ă���Ɣ��j����Ă��܂�\n�ǂ����ɃV�[���h�͂Ȃ����낤��";
                break;

            case "4":
                infoTxt.text = "���ʂȕ�������W���Ă���\n�U���̌��ʂ��o���Ă�����";
                break;

            case "5":
                infoTxt.text = "�łōU���͂��������Ă��܂�\n��������g���悤";
                break;

            case "6":
                infoTxt.text = "������̍s�����R�s�[�����\n���Η͂̃J�[�h���g���Ƃ��͒��ӂ��悤";
                break;

            case "7":
                infoTxt.text = "���˃o���A�ōU���𒵂˕Ԃ��Ă���\n�O�X�e�[�W���l�J�[�h�I�т͐T�d��";
                break;

            case "8":
                infoTxt.text = "�h��͍ő��...�h��H";
                break;

            case "9":
                infoTxt.text = "�w�؂����肻����...\n�ŏ��̖ҍU��˔j�ł���Ώ����؂͌����Ă��邾�낤";
                break;

            case "10":
                infoTxt.text = "���o�[�W�����ł͍Ō�̓G�ƂȂ�\n�_�C�i�}�C�g�ł��Ԃ��Ă�낤\n...����������������Ă�l�͂���̂��H";
                break;
        }
    }

    /// <summary>
    /// Close Explain Panel
    /// </summary>
    public void exitExplain()
    {
        infoPanel.SetActive(false);
    }

    /// <summary>
    /// �f�b�L�\�z�p�l�����J������
    /// </summary>
    public void openDeckBuildPanel()
    {
        deckBuildPanel.SetActive(true);
        buildPanel.SetActive(true);

        if (isClick == false)
        {
            UpdateDeck(deckData.GetDeck());
        }
        else
        {
            UpdateDeck(deckData.GetDefenceDeck());
        }
        helpPanel.SetActive(false);
        cardViewPanel.SetActive(false);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);
        iconPanel.SetActive(false);
    }

    /// <summary>
    /// �f�b�L�\�z�p�l������鏈��
    /// </summary>
    public void closeDeckBuildPanel()
    {
        deckBuildPanel.SetActive(false);
    }

    /// <summary>
    /// �J�[�h�r���[�p�l������
    /// </summary>
    public void openViewPanel()
    {
        cardViewPanel.SetActive(true);

        CardSet();
        buildPanel.SetActive(false);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);
        helpPanel.SetActive(false);
        iconPanel.SetActive(false);
    }

    /// <summary>
    /// �ڍ׃p�l������
    /// </summary>
    public void openHelpPanel()
    {    
        helpPanel.SetActive(true);
        
        cardViewPanel.SetActive(false);
        buildPanel.SetActive(false);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);
        iconPanel.SetActive(false);
    }


    /// <summary>
    /// �r���h��ʂɖ߂鏈��
    /// </summary>
    public void backBuildPanel()
    {
        buildPanel.SetActive(true);

        cardViewPanel.SetActive(false);
        helpPanel.SetActive(false);

        if (isClick == false)
        {
            deckData.SetDeck();
        }
        else
        {
            deckData.SetDefenceDeck();
        }
        warning.text = "";
    }

    /// <summary>
    /// �g�p�\�J�[�h�ꗗ�p�l���Q�Ə���
    /// </summary>
    public void openShowCardPanel()
    {
        showCardPanel.SetActive(true);
        SetUsableView();
        deckBuildPanel.SetActive(false);
        infoPanel.SetActive(false);
        iconPanel.SetActive(false);
    }

    /// <summary>
    /// �g�p�\�J�[�h�ꗗ�p�l������鏈��
    /// </summary>
    public void closeCardPanel()
    {
        showCardPanel.SetActive(false);
    }

    /// <summary>
    /// Load Battle Scene
    /// </summary>
    public void goFight()
    {
        SceneManager.LoadScene("Battle");
    }


    /// <summary>
    /// Check Usable Card
    /// </summary>
    /// <param name="name"></param>
    /// <param name="stack"></param>
    public void CardClick(string name, string stack)
    {
        switch (name)
        {
            case "Sword":
                infoText.text = name + ":1�_���[�W��^���� �����~" + stack;
                break;

            case "S.Y.T.H":
                infoText.text = name + ":2�_���[�W��^���� �����~" + stack;
                break;

            case "A.X.E":
                infoText.text = name + ":1�_���[�W��^���� �u���b�N�𖳎����j�� �����~" + stack;
                break;

            case "M.A.C.E":
                infoText.text = name + ":1�_���[�W�ɉ����u���b�N�̒l���_���[�W��^���� �����~" + stack;
                break;

            case "Shield":
                infoText.text = name + ":1�u���b�N���󂯂� �����~" + stack;
                break;

            case "ForgeHammer":
                infoText.text = name + ":1�_���[�W��^���� ���̍s���̍U����+1 �����~" + stack;
                break;

            case "Injector":
                infoText.text = name + ":1�_���[�W��^���� �G���o��������(�s����1�_���[�W) �����~" + stack;
                break;

            case "PoisonKnife":
                infoText.text = name + ":1�_���[�W��^���� �G�̍U����-1 �����~" + stack;
                break;

            case "6mmBullet":
                infoText.text = name + ":3�_���[�W��^���� ...�e������΂̘b �����~" + stack;
                break;

            case "SwatShield":
                infoText.text = name + ":2�u���b�N���󂯂� �����~" + stack;
                break;

            default:
                break;
        }
    }

    void SetUsableView()
    {
        int cnt = 0;
        // Create Stage Button from Server
        GameObject cardObj = new GameObject();

        foreach (var item in deckData.cardDictionary.Keys)
        {
            // �L�[�𕶎���ɕϊ�
            string cardName = item.ToString();
            // �X�^�b�N���𐔎��ɕϊ�
            int.TryParse(deckData.cardDictionary[cardName].Stack, out int cardStack);

            GameObject obj = (GameObject)Resources.Load("UI/" + cardName);

            if (cnt < 5)
            {
                cardObj = Instantiate(obj, new Vector2(-400 + (200 * cnt), 100), Quaternion.identity);
            }
            else
            {
                cardObj = Instantiate(obj, new Vector2(-300 + (200 * (cnt - 5)), -130), Quaternion.identity);
            }

            cardObj.name = cardName;

            cardObj.transform.SetParent(this.cardParent.transform, false);
            cardObj.GetComponent<Button>().onClick.AddListener(() => CardClick(cardName, deckData.cardDictionary[cardName].Stack));
            cnt++;
        }
    }

    async void Loading()
    {
        loadingPanel.SetActive(true);

        float angle = 8;
        bool rot = true;

        for (int i = 0; i < 80; i++)
        {
            if (rot)
            {
                loadingIcon.transform.rotation *= Quaternion.AngleAxis(angle, Vector3.back);
            }
            else
            {
                loadingIcon.transform.rotation *= Quaternion.AngleAxis(angle, Vector3.forward);
            }
            await Task.Delay(10);
        }
        loadingPanel.SetActive(false);
    }

    public void OpenIconPanel()
    {
        iconPanel.SetActive(true);

        helpPanel.SetActive(true);
        cardViewPanel.SetActive(false);
        buildPanel.SetActive(false);
        infoPanel.SetActive(false);
        showCardPanel.SetActive(false);
    }

    public void CloseIconPanel()
    {
        iconPanel.SetActive(false);
    }

    /// <summary>
    /// ���̃A�C�R���I������
    /// </summary>
    public void NextIcon()
    {  
        iconNum++;
        if (iconNum >= 10) iconNum = 0;

        // �I�u�W�F�N�g�̎擾
        Image preview = GameObject.Find("IconPreview").GetComponent<Image>();
        // ���\�[�X����A�A�C�R�����擾
        Texture2D texture = Resources.Load("Icons/icon00" + iconNum) as Texture2D;

        iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                       Vector2.zero);
        preview.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                               Vector2.zero);

        iconName = texture.name;  
    }

    /// <summary>
    /// �O�̃A�C�R���I������
    /// </summary>
    public void BackIcon()
    {
        iconNum--;
        if (iconNum <= -1) iconNum = 9;

        // �I�u�W�F�N�g�̎擾
        Image preview = GameObject.Find("IconPreview").GetComponent<Image>();
        // ���\�[�X����A�A�C�R�����擾
        Texture2D texture = Resources.Load("Icons/icon00" + iconNum) as Texture2D;

        iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                       Vector2.zero);
        preview.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                       Vector2.zero);

        iconName = texture.name;
    }

    /// <summary>
    /// �v���t�B�[���X�V
    /// </summary>
    public void updateDisplayInfo()
    {
        Debug.Log("�A�C�R����:" + iconName);
        Debug.Log("�v���C���[��:" + playerName.text); ;
        StartCoroutine(NetworkManager.Instance.UpdateProfile(playerName.text, iconName));
    }
}
